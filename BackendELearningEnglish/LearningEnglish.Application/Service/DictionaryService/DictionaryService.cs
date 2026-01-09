using System.Text.Json;
using System.Text;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Application.Cofigurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Service
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DictionaryService> _logger;
        private readonly IConfiguration _configuration;
        private readonly OxfordDictionaryOptions _oxfordOptions;
        private readonly UnsplashOptions _unsplashOptions;
        private readonly IMinioFileStorage _minioService;
        private readonly IAzureSpeechService _azureSpeechService;
        private readonly IFlashCardMediaService _flashCardMediaService;
        private const string FREE_DICTIONARY_API = "https://api.dictionaryapi.dev/api/v2/entries/en";
        private const string GOOGLE_TRANSLATE_API = "https://translate.googleapis.com/translate_a/single";

        public DictionaryService(
            IHttpClientFactory httpClientFactory,
            ILogger<DictionaryService> logger,
            IConfiguration configuration,
            IOptions<OxfordDictionaryOptions> oxfordOptions,
            IOptions<UnsplashOptions> unsplashOptions,
            IMinioFileStorage minioService,
            IAzureSpeechService azureSpeechService,
            IFlashCardMediaService flashCardMediaService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _oxfordOptions = oxfordOptions.Value;
            _unsplashOptions = unsplashOptions.Value;
            _minioService = minioService;
            _azureSpeechService = azureSpeechService;
            _flashCardMediaService = flashCardMediaService;
        }

        public async Task<ServiceResponse<DictionaryLookupResultDto>> LookupWordAsync(string word, string? targetLanguage = "vi")
        {
            var response = new ServiceResponse<DictionaryLookupResultDto>();

            try
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    response.Success = false;
                    response.Message = "Word cannot be empty";
                    return response;
                }

                // Try Oxford API first (more reliable)
                var oxfordResult = await LookupWordFromOxfordAsync(word);
                if (oxfordResult.Success && oxfordResult.Data != null)
                {
                    response = oxfordResult;
                    return response;
                }

                // Fallback to Free Dictionary API
                _logger.LogInformation("Oxford API unavailable, falling back to Free Dictionary API for word: {Word}", word);

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var apiUrl = $"{FREE_DICTIONARY_API}/{word.Trim().ToLower()}";
                var apiResponse = await client.GetAsync(apiUrl);

                if (!apiResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Dictionary API returned {StatusCode} for word: {Word}", apiResponse.StatusCode, word);
                    response.Success = false;
                    response.Message = $"Word '{word}' not found in dictionary";
                    return response;
                }

                var jsonContent = await apiResponse.Content.ReadAsStringAsync();
                var dictionaryData = JsonSerializer.Deserialize<List<DictionaryApiResponse>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dictionaryData == null || dictionaryData.Count == 0)
                {
                    response.Success = false;
                    response.Message = "No data found for this word";
                    return response;
                }

                var firstEntry = dictionaryData.First();
                var result = new DictionaryLookupResultDto
                {
                    Word = firstEntry.Word ?? word,
                    Phonetic = firstEntry.Phonetic ?? firstEntry.Phonetics?.FirstOrDefault()?.Text,
                    SourceUrl = firstEntry.SourceUrls?.FirstOrDefault()
                };

                // Parse meanings
                if (firstEntry.Meanings != null)
                {
                    foreach (var meaning in firstEntry.Meanings)
                    {
                        var meaningDto = new DictionaryMeaningDto
                        {
                            PartOfSpeech = meaning.PartOfSpeech ?? "unknown"
                        };

                        if (meaning.Definitions != null)
                        {
                            foreach (var def in meaning.Definitions.Take(3)) // Top 3 definitions
                            {
                                meaningDto.Definitions.Add(new DictionaryDefinitionDto
                                {
                                    Definition = def.Definition ?? "",
                                    Example = def.Example
                                });
                            }
                        }

                        if (meaning.Synonyms != null)
                        {
                            meaningDto.Synonyms = meaning.Synonyms.Take(10).ToList();
                        }

                        if (meaning.Antonyms != null)
                        {
                            meaningDto.Antonyms = meaning.Antonyms.Take(10).ToList();
                        }

                        result.Meanings.Add(meaningDto);
                    }
                }

                response.Data = result;
                response.Message = "Word lookup successful";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during word lookup: {Word}", word);
                response.Success = false;
                response.Message = "Dictionary service unavailable";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up word: {Word}", word);
                response.Success = false;
                response.Message = "An error occurred during word lookup";
            }

            return response;
        }
        // gen flashcard from word 

        public async Task<ServiceResponse<GenerateFlashCardPreviewResponseDto>> GenerateFlashCardFromWordAsync(string word)
        {
            var response = new ServiceResponse<GenerateFlashCardPreviewResponseDto>();

            try
            {
                // Lookup word in dictionary with audio URL extraction
                var lookupResult = await LookupWordWithAudioAsync(word, "vi");

                if (!lookupResult.Success || lookupResult.Data == null)
                {
                    response.Success = false;
                    response.Message = lookupResult.Message;
                    return response;
                }

                var dictData = lookupResult.Data;
                var flashCard = new Domain.Entities.FlashCard
                {
                    Word = dictData.Word,
                    Pronunciation = dictData.Phonetic
                };

                // Handle audio generation with priority: Oxford audio → Azure TTS → null
                string? audioTempKey = null;
                if (!string.IsNullOrEmpty(dictData.AudioUrl))
                {
                    // Oxford provided audio URL, download and upload to MinIO
                    try
                    {
                        var audioStream = await DownloadAudioFromUrlAsync(dictData.AudioUrl);
                        if (audioStream != null)
                        {
                            audioTempKey = await UploadAudioToMinioAsync(audioStream, $"{word}.mp3");
                            _logger.LogInformation("Successfully uploaded Oxford audio for word: {Word}", word);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to download/upload Oxford audio for word: {Word}", word);
                    }
                }

                // Fallback to Azure TTS if Oxford audio failed
                if (string.IsNullOrEmpty(audioTempKey))
                {
                    try
                    {
                        var ttsStream = await _azureSpeechService.GenerateSpeechAsync(word, "en-US", "en-US-JennyNeural");
                        if (ttsStream != null)
                        {
                            audioTempKey = await UploadAudioToMinioAsync(ttsStream, $"{word}_tts.mp3");
                            _logger.LogInformation("Successfully generated and uploaded Azure TTS audio for word: {Word}", word);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate Azure TTS audio for word: {Word}", word);
                    }
                }

                flashCard.AudioKey = audioTempKey;

                // Handle image generation from Unsplash
                string? imageTempKey = null;
                try
                {
                    var imageStream = await SearchAndDownloadImageFromUnsplashAsync(word);
                    if (imageStream != null)
                    {
                        imageTempKey = await UploadImageToMinioAsync(imageStream, $"{word}.jpg");
                        _logger.LogInformation("Successfully uploaded Unsplash image for word: {Word}", word);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to download/upload Unsplash image for word: {Word}", word);
                }

                flashCard.ImageKey = imageTempKey;

                // Get first meaning as primary
                var primaryMeaning = dictData.Meanings.FirstOrDefault();
                if (primaryMeaning != null)
                {
                    flashCard.PartOfSpeech = primaryMeaning.PartOfSpeech;

                    // Get first definition as meaning
                    var primaryDef = primaryMeaning.Definitions.FirstOrDefault();
                    if (primaryDef != null)
                    {
                        flashCard.Meaning = primaryDef.Definition;
                        flashCard.Example = primaryDef.Example;

                        // Translate definition to Vietnamese using simple translation service
                        var translationResult = await TranslateTextAsync(primaryDef.Definition, "vi");
                        if (translationResult.Success && !string.IsNullOrEmpty(translationResult.Data))
                        {
                            flashCard.Meaning = translationResult.Data;
                        }

                        // Translate example if exists
                        if (!string.IsNullOrEmpty(primaryDef.Example))
                        {
                            var exampleTranslation = await TranslateTextAsync(primaryDef.Example, "vi");
                            if (exampleTranslation.Success && !string.IsNullOrEmpty(exampleTranslation.Data))
                            {
                                flashCard.ExampleTranslation = exampleTranslation.Data;
                            }
                        }
                    }

                    // Combine all synonyms
                    var allSynonyms = dictData.Meanings
                        .SelectMany(m => m.Synonyms)
                        .Distinct()
                        .Take(10)
                        .ToList();

                    if (allSynonyms.Count != 0)
                    {
                        flashCard.Synonyms = JsonSerializer.Serialize(allSynonyms);
                    }

                    // Combine all antonyms
                    var allAntonyms = dictData.Meanings
                        .SelectMany(m => m.Antonyms)
                        .Distinct()
                        .Take(10)
                        .ToList();

                    if (allAntonyms.Count != 0)
                    {
                        flashCard.Antonyms = JsonSerializer.Serialize(allAntonyms);
                    }
                }

                // Map to GenerateFlashCardPreviewResponseDto with URLs and temp keys
                var previewDto = new GenerateFlashCardPreviewResponseDto
                {
                    Word = flashCard.Word,
                    Pronunciation = flashCard.Pronunciation,
                    PartOfSpeech = flashCard.PartOfSpeech,
                    Meaning = flashCard.Meaning,
                    Example = flashCard.Example,
                    ExampleTranslation = flashCard.ExampleTranslation,
                    Synonyms = flashCard.Synonyms,
                    Antonyms = flashCard.Antonyms,
                    // URLs for preview
                    AudioUrl = !string.IsNullOrWhiteSpace(flashCard.AudioKey) ? _flashCardMediaService.BuildAudioUrl(flashCard.AudioKey) : null,
                    ImageUrl = !string.IsNullOrWhiteSpace(flashCard.ImageKey) ? _flashCardMediaService.BuildImageUrl(flashCard.ImageKey) : null,
                    // Temp keys for create operation
                    AudioTempKey = flashCard.AudioKey,
                    ImageTempKey = flashCard.ImageKey
                };

                response.Data = previewDto;
                response.Message = "FlashCard generated successfully for review";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating FlashCard from word: {Word}", word);
                response.Success = false;
                response.Message = "An error occurred while generating FlashCard data";
            }

            return response;
        }

        private async Task<ServiceResponse<string>> TranslateTextAsync(string text, string targetLanguage)
        {
            var response = new ServiceResponse<string>();

            try
            {
                // Simple Google Translate API (free, no auth)
                var client = _httpClientFactory.CreateClient();
                var url = $"{GOOGLE_TRANSLATE_API}?client=gtx&sl=en&tl={targetLanguage}&dt=t&q={Uri.EscapeDataString(text)}";

                var apiResponse = await client.GetAsync(url);
                if (apiResponse.IsSuccessStatusCode)
                {
                    var content = await apiResponse.Content.ReadAsStringAsync();
                    // Parse Google Translate response (format: [[["translation","source",null,null,0]]])
                    var translatedText = ParseGoogleTranslateResponse(content);

                    response.Data = translatedText ?? text;
                    response.Message = "Translation successful";
                }
                else
                {
                    response.Data = text; // Fallback to original
                    response.Message = "Translation unavailable, using original text";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Translation failed, using original text");
                response.Data = text; // Fallback
            }

            return response;
        }

        private static string? ParseGoogleTranslateResponse(string json)
        {
            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var firstArray = root[0];
                    if (firstArray.ValueKind == JsonValueKind.Array && firstArray.GetArrayLength() > 0)
                    {
                        var translation = firstArray[0];
                        if (translation.ValueKind == JsonValueKind.Array && translation.GetArrayLength() > 0)
                        {
                            return translation[0].GetString();
                        }
                    }
                }
            }
            catch
            {
                // Ignore parse errors
            }
            return null;
        }

        private async Task<ServiceResponse<DictionaryLookupResultDto>> LookupWordWithAudioAsync(string word, string? targetLanguage = "vi")
        {
            var response = await LookupWordAsync(word, targetLanguage);

            // Try to extract audio URL from Oxford or Free Dictionary
            if (response.Success && response.Data != null)
            {
                var audioUrl = await ExtractAudioUrlAsync(word);
                response.Data.AudioUrl = audioUrl;
            }

            return response;
        }

        private async Task<string?> ExtractAudioUrlAsync(string word)
        {
            try
            {
                // Try Oxford API first
                if (!string.IsNullOrWhiteSpace(_oxfordOptions.AppId) && !string.IsNullOrWhiteSpace(_oxfordOptions.AppKey))
                {
                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("app_id", _oxfordOptions.AppId);
                    client.DefaultRequestHeaders.Add("app_key", _oxfordOptions.AppKey);
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var apiUrl = $"{_oxfordOptions.BaseUrl}/entries/en-us/{word.Trim().ToLower()}";
                    var apiResponse = await client.GetAsync(apiUrl);

                    if (apiResponse.IsSuccessStatusCode)
                    {
                        var jsonContent = await apiResponse.Content.ReadAsStringAsync();
                        var oxfordData = JsonSerializer.Deserialize<OxfordApiResponse>(jsonContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        // Extract first audio file URL
                        var audioFile = oxfordData?.Results
                            ?.SelectMany(r => r.LexicalEntries ?? new List<OxfordLexicalEntry>())
                            ?.SelectMany(le => le.Pronunciations ?? new List<OxfordPronunciation>())
                            ?.FirstOrDefault(p => !string.IsNullOrEmpty(p.AudioFile))
                            ?.AudioFile;

                        if (!string.IsNullOrEmpty(audioFile))
                        {
                            return audioFile;
                        }
                    }
                }

                // Fallback to Free Dictionary API
                var freeDictClient = _httpClientFactory.CreateClient();
                var freeDictUrl = $"{FREE_DICTIONARY_API}/{word.Trim().ToLower()}";
                var freeDictResponse = await freeDictClient.GetAsync(freeDictUrl);

                if (freeDictResponse.IsSuccessStatusCode)
                {
                    var jsonContent = await freeDictResponse.Content.ReadAsStringAsync();
                    var dictionaryData = JsonSerializer.Deserialize<List<DictionaryApiResponse>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var audioUrl = dictionaryData?.FirstOrDefault()?.Phonetics
                        ?.FirstOrDefault(p => !string.IsNullOrEmpty(p.Audio))
                        ?.Audio;

                    if (!string.IsNullOrEmpty(audioUrl))
                    {
                        return audioUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract audio URL for word: {Word}", word);
            }

            return null;
        }

        private async Task<Stream?> DownloadAudioFromUrlAsync(string audioUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(audioUrl);

                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    return new MemoryStream(bytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download audio from URL: {Url}", audioUrl);
            }

            return null;
        }

        private async Task<string?> UploadAudioToMinioAsync(Stream audioStream, string fileName)
        {
            try
            {
                // Convert Stream to IFormFile for MinIO upload
                var memoryStream = new MemoryStream();
                await audioStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "audio", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "audio/mpeg"
                };

                // Sử dụng FlashCardMediaService - không cần biết bucket/folder
                return await _flashCardMediaService.UploadTempAudioAsync(formFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload audio to MinIO: {FileName}", fileName);
            }

            return null;
        }

        private async Task<ServiceResponse<DictionaryLookupResultDto>> LookupWordFromOxfordAsync(string word)
        {
            var response = new ServiceResponse<DictionaryLookupResultDto>();

            try
            {
                if (string.IsNullOrWhiteSpace(_oxfordOptions.AppId) || string.IsNullOrWhiteSpace(_oxfordOptions.AppKey))
                {
                    response.Success = false;
                    response.Message = "Oxford API credentials not configured";
                    return response;
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("app_id", _oxfordOptions.AppId);
                client.DefaultRequestHeaders.Add("app_key", _oxfordOptions.AppKey);
                client.Timeout = TimeSpan.FromSeconds(10);

                // Oxford API endpoint: /entries/{source_lang}/{word_id}
                var apiUrl = $"{_oxfordOptions.BaseUrl}/entries/en-us/{word.Trim().ToLower()}";
                var apiResponse = await client.GetAsync(apiUrl);

                if (!apiResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Oxford API returned {StatusCode} for word: {Word}", apiResponse.StatusCode, word);
                    response.Success = false;
                    response.Message = $"Word '{word}' not found in Oxford dictionary";
                    return response;
                }

                var jsonContent = await apiResponse.Content.ReadAsStringAsync();
                var oxfordData = JsonSerializer.Deserialize<OxfordApiResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (oxfordData?.Results == null || oxfordData.Results.Count == 0)
                {
                    response.Success = false;
                    response.Message = "No data found in Oxford dictionary";
                    return response;
                }

                var result = new DictionaryLookupResultDto
                {
                    Word = oxfordData.Id ?? word
                };

                // Parse lexical entries
                foreach (var oxfordResult in oxfordData.Results)
                {
                    if (oxfordResult.LexicalEntries == null) continue;

                    foreach (var lexEntry in oxfordResult.LexicalEntries)
                    {
                        var meaningDto = new DictionaryMeaningDto
                        {
                            PartOfSpeech = lexEntry.LexicalCategory?.Text ?? "unknown"
                        };

                        // Get pronunciation
                        if (lexEntry.Pronunciations != null && lexEntry.Pronunciations.Count != 0)
                        {
                            result.Phonetic = lexEntry.Pronunciations.First().PhoneticSpelling;
                        }

                        // Parse entries
                        if (lexEntry.Entries != null)
                        {
                            foreach (var entry in lexEntry.Entries)
                            {
                                if (entry.Senses == null) continue;

                                foreach (var sense in entry.Senses.Take(3))
                                {
                                    var definition = sense.Definitions?.FirstOrDefault();
                                    if (!string.IsNullOrEmpty(definition))
                                    {
                                        meaningDto.Definitions.Add(new DictionaryDefinitionDto
                                        {
                                            Definition = definition,
                                            Example = sense.Examples?.FirstOrDefault()?.Text
                                        });
                                    }

                                    // Collect synonyms
                                    if (sense.Synonyms != null)
                                    {
                                        meaningDto.Synonyms.AddRange(sense.Synonyms.Select(s => s.Text ?? "").Where(t => !string.IsNullOrEmpty(t)));
                                    }

                                    // Collect antonyms
                                    if (sense.Antonyms != null)
                                    {
                                        meaningDto.Antonyms.AddRange(sense.Antonyms.Select(a => a.Text ?? "").Where(t => !string.IsNullOrEmpty(t)));
                                    }
                                }
                            }
                        }

                        if (meaningDto.Definitions.Count != 0)
                        {
                            result.Meanings.Add(meaningDto);
                        }
                    }
                }

                if (result.Meanings.Count == 0)
                {
                    response.Success = false;
                    response.Message = "No definitions found in Oxford dictionary";
                    return response;
                }

                response.Data = result;
                response.Message = "Word lookup successful from Oxford Dictionary";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during Oxford API lookup: {Word}", word);
                response.Success = false;
                response.Message = "Oxford Dictionary service unavailable";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up word from Oxford API: {Word}", word);
                response.Success = false;
                response.Message = "An error occurred during Oxford dictionary lookup";
            }

            return response;
        }

        private async Task<Stream?> SearchAndDownloadImageFromUnsplashAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_unsplashOptions.AccessKey))
                {
                    _logger.LogWarning("Unsplash Access Key not configured");
                    return null;
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_unsplashOptions.AccessKey}");
                client.Timeout = TimeSpan.FromSeconds(15);

                // Search for images
                var searchUrl = $"{_unsplashOptions.BaseUrl}/search/photos?query={Uri.EscapeDataString(query)}&per_page=1&orientation=landscape";
                var searchResponse = await client.GetAsync(searchUrl);

                if (!searchResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Unsplash search failed with status: {StatusCode} for query: {Query}",
                        searchResponse.StatusCode, query);
                    return null;
                }

                var searchContent = await searchResponse.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<UnsplashSearchResponse>(searchContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (searchResult?.Results == null || searchResult.Results.Count == 0)
                {
                    _logger.LogWarning("No images found on Unsplash for query: {Query}", query);
                    return null;
                }

                // Get first image URL (regular size - good for flashcards)
                var firstImage = searchResult.Results.First();
                var imageUrl = firstImage.Urls?.Regular;

                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogWarning("Image URL is empty from Unsplash for query: {Query}", query);
                    return null;
                }

                _logger.LogInformation("Found Unsplash image for '{Query}': {Url}", query, imageUrl);

                // Download image
                var imageResponse = await client.GetAsync(imageUrl);
                if (imageResponse.IsSuccessStatusCode)
                {
                    var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                    return new MemoryStream(imageBytes);
                }

                _logger.LogWarning("Failed to download Unsplash image from URL: {Url}", imageUrl);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching/downloading image from Unsplash for query: {Query}", query);
                return null;
            }
        }

        private async Task<string?> UploadImageToMinioAsync(Stream imageStream, string fileName)
        {
            try
            {
                // Convert Stream to IFormFile for MinIO upload
                var memoryStream = new MemoryStream();
                await imageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "image", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                };

                // Sử dụng FlashCardMediaService - không cần biết bucket/folder
                return await _flashCardMediaService.UploadTempImageAsync(formFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image to MinIO: {FileName}", fileName);
            }

            return null;
        }
    }
}

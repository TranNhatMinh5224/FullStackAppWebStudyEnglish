using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearningEnglish.Infrastructure.Services
{
    public class AzureSpeechService : IAzureSpeechService
    {
        private readonly string _subscriptionKey;
        private readonly string _region;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AzureSpeechService> _logger;

        public AzureSpeechService(
            IConfiguration configuration, 
            HttpClient httpClient,
            ILogger<AzureSpeechService> logger)
        {
            _subscriptionKey = configuration["AzureSpeech:SubscriptionKey"] ?? 
                throw new ArgumentNullException("AzureSpeech:SubscriptionKey is not configured");
            _region = configuration["AzureSpeech:Region"] ?? 
                throw new ArgumentNullException("AzureSpeech:Region is not configured");
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AzureSpeechAssessmentResult> AssessPronunciationAsync(
            string audioUrl, 
            string referenceText, 
            string locale = "en-US")
        {
            try
            {
                // Download audio from MinIO URL
                var audioBytes = await _httpClient.GetByteArrayAsync(audioUrl);
                using var audioStream = new MemoryStream(audioBytes);
                
                return await AssessPronunciationFromStreamAsync(audioStream, referenceText, locale);
            }
            catch (Exception ex)
            {
                return new AzureSpeechAssessmentResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to download audio: {ex.Message}"
                };
            }
        }

        public async Task<AzureSpeechAssessmentResult> AssessPronunciationFromStreamAsync(
            Stream audioStream, 
            string referenceText, 
            string locale = "en-US")
        {
            try
            {
                var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                config.SpeechRecognitionLanguage = locale;

                // Set up audio stream
                byte[] audioBytes;
                if (audioStream is MemoryStream ms)
                {
                    audioBytes = ms.ToArray();
                }
                else
                {
                    using var memStream = new MemoryStream();
                    await audioStream.CopyToAsync(memStream);
                    audioBytes = memStream.ToArray();
                }

                using var audioInputStream = AudioInputStream.CreatePushStream();
                using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
                using var recognizer = new SpeechRecognizer(config, audioConfig);

                // Configure pronunciation assessment via JSON
                var pronunciationAssessmentJson = $@"{{
                    ""ReferenceText"": ""{referenceText}"",
                    ""GradingSystem"": ""HundredMark"",
                    ""Granularity"": ""Phoneme"",
                    ""EnableMiscue"": true
                }}";
                
                recognizer.Properties.SetProperty("SPEECH-PronunciationAssessment", pronunciationAssessmentJson);

                // Push audio data
                audioInputStream.Write(audioBytes);
                audioInputStream.Close();

                // Recognize speech
                var result = await recognizer.RecognizeOnceAsync();

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    // Parse JSON result
                    var jsonResult = result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                    
                    // Try to extract scores from JSON
                    try
                    {
                        var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonResult);
                        var root = jsonDoc.RootElement;
                        
                        double accuracyScore = 0;
                        double fluencyScore = 0;
                        double completenessScore = 0;
                        double pronScore = 0;
                        
                        // 🆕 Word-level analysis
                        var words = new List<WordPronunciationDetail>();
                        var allPhonemeScores = new Dictionary<string, List<double>>();
                        
                        if (root.TryGetProperty("NBest", out var nBest) && nBest.GetArrayLength() > 0)
                        {
                            var firstResult = nBest[0];
                            
                            // Overall scores
                            if (firstResult.TryGetProperty("PronunciationAssessment", out var pronAssessment))
                            {
                                if (pronAssessment.TryGetProperty("AccuracyScore", out var acc))
                                    accuracyScore = acc.GetDouble();
                                if (pronAssessment.TryGetProperty("FluencyScore", out var flu))
                                    fluencyScore = flu.GetDouble();
                                if (pronAssessment.TryGetProperty("CompletenessScore", out var comp))
                                    completenessScore = comp.GetDouble();
                                if (pronAssessment.TryGetProperty("PronScore", out var pron))
                                    pronScore = pron.GetDouble();
                            }
                            
                            // 🆕 Parse words array
                            if (firstResult.TryGetProperty("Words", out var wordsArray))
                            {
                                foreach (var wordElement in wordsArray.EnumerateArray())
                                {
                                    var wordDetail = new WordPronunciationDetail
                                    {
                                        Word = wordElement.TryGetProperty("Word", out var wordProp) 
                                            ? wordProp.GetString() ?? "" : "",
                                        Offset = wordElement.TryGetProperty("Offset", out var offset) 
                                            ? offset.GetInt32() : 0,
                                        Duration = wordElement.TryGetProperty("Duration", out var duration) 
                                            ? duration.GetInt32() : 0
                                    };
                                    
                                    // Parse word-level pronunciation assessment
                                    if (wordElement.TryGetProperty("PronunciationAssessment", out var wordPronAssessment))
                                    {
                                        wordDetail.AccuracyScore = wordPronAssessment.TryGetProperty("AccuracyScore", out var wordAcc)
                                            ? wordAcc.GetDouble() : 0;
                                        wordDetail.ErrorType = wordPronAssessment.TryGetProperty("ErrorType", out var errorType)
                                            ? errorType.GetString() ?? "None" : "None";
                                    }
                                    
                                    // 🆕 Parse phonemes for each word
                                    if (wordElement.TryGetProperty("Phonemes", out var phonemesArray))
                                    {
                                        foreach (var phonemeElement in phonemesArray.EnumerateArray())
                                        {
                                            var phoneme = phonemeElement.TryGetProperty("Phoneme", out var phonemeProp)
                                                ? phonemeProp.GetString() ?? "" : "";
                                            var phonemeScore = 0.0;
                                            
                                            if (phonemeElement.TryGetProperty("PronunciationAssessment", out var phonemePronAssessment))
                                            {
                                                phonemeScore = phonemePronAssessment.TryGetProperty("AccuracyScore", out var pScore)
                                                    ? pScore.GetDouble() : 0;
                                            }
                                            
                                            var phonemeDetail = new PhonemeDetail
                                            {
                                                Phoneme = phoneme,
                                                PhonemeDisplay = ConvertToDisplayPhoneme(phoneme),
                                                AccuracyScore = phonemeScore,
                                                Offset = phonemeElement.TryGetProperty("Offset", out var pOffset) 
                                                    ? pOffset.GetInt32() : 0,
                                                Duration = phonemeElement.TryGetProperty("Duration", out var pDuration) 
                                                    ? pDuration.GetInt32() : 0
                                            };
                                            
                                            wordDetail.Phonemes.Add(phonemeDetail);
                                            
                                            // Track phoneme scores for analysis
                                            if (!string.IsNullOrEmpty(phoneme))
                                            {
                                                if (!allPhonemeScores.ContainsKey(phoneme))
                                                    allPhonemeScores[phoneme] = new List<double>();
                                                allPhonemeScores[phoneme].Add(phonemeScore);
                                            }
                                        }
                                    }
                                    
                                    words.Add(wordDetail);
                                }
                            }
                        }
                        
                        // 🆕 Analyze problem and strong phonemes
                        var problemPhonemes = allPhonemeScores
                            .Where(p => p.Value.Any() && p.Value.Average() < 70)
                            .OrderBy(p => p.Value.Average())
                            .Select(p => ConvertToDisplayPhoneme(p.Key))
                            .Take(5)
                            .ToList();

                        var strongPhonemes = allPhonemeScores
                            .Where(p => p.Value.Any() && p.Value.Average() >= 85)
                            .OrderByDescending(p => p.Value.Average())
                            .Select(p => ConvertToDisplayPhoneme(p.Key))
                            .Take(5)
                            .ToList();
                        
                        return new AzureSpeechAssessmentResult
                        {
                            Success = true,
                            AccuracyScore = accuracyScore,
                            FluencyScore = fluencyScore,
                            CompletenessScore = completenessScore,
                            PronunciationScore = pronScore,
                            RecognizedText = result.Text,
                            DetailedResultJson = jsonResult,
                            RawResponse = jsonResult,
                            Words = words,
                            ProblemPhonemes = problemPhonemes,
                            StrongPhonemes = strongPhonemes
                        };
                    }
                    catch (Exception parseEx)
                    {
                        _logger.LogError(parseEx, "Error parsing Azure speech result");
                        // If parsing fails, return what we have
                        return new AzureSpeechAssessmentResult
                        {
                            Success = true,
                            AccuracyScore = 0,
                            FluencyScore = 0,
                            CompletenessScore = 0,
                            PronunciationScore = 0,
                            RecognizedText = result.Text,
                            DetailedResultJson = jsonResult,
                            RawResponse = jsonResult,
                            ErrorMessage = $"Speech recognized but score parsing failed: {parseEx.Message}"
                        };
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    return new AzureSpeechAssessmentResult
                    {
                        Success = false,
                        ErrorMessage = "No speech could be recognized from the audio."
                    };
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    return new AzureSpeechAssessmentResult
                    {
                        Success = false,
                        ErrorMessage = $"Speech recognition canceled: {cancellation.Reason}. Details: {cancellation.ErrorDetails}"
                    };
                }
                else
                {
                    return new AzureSpeechAssessmentResult
                    {
                        Success = false,
                        ErrorMessage = $"Unexpected result reason: {result.Reason}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AzureSpeechAssessmentResult
                {
                    Success = false,
                    ErrorMessage = $"Azure Speech Service error: {ex.Message}"
                };
            }
        }

        public async Task<Stream?> GenerateSpeechAsync(
            string text, 
            string locale = "en-US", 
            string voiceName = "en-US-JennyNeural")
        {
            try
            {
                var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                config.SpeechSynthesisVoiceName = voiceName;
                config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

                using var synthesizer = new SpeechSynthesizer(config, null);
                var result = await synthesizer.SpeakTextAsync(text);

                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    _logger.LogInformation("Speech synthesized for text: {Text}", text);
                    var audioData = result.AudioData;
                    return new MemoryStream(audioData);
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    _logger.LogError("Speech synthesis canceled: {Reason}. Details: {ErrorDetails}", 
                        cancellation.Reason, cancellation.ErrorDetails);
                    return null;
                }
                else
                {
                    _logger.LogWarning("Speech synthesis failed with reason: {Reason}", result.Reason);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating speech for text: {Text}", text);
                return null;
            }
        }

        // 🆕 Helper method: Convert IPA phoneme symbols to user-friendly display format
        private string ConvertToDisplayPhoneme(string ipaPhoneme)
        {
            if (string.IsNullOrEmpty(ipaPhoneme))
                return ipaPhoneme;

            // Map IPA symbols to readable format
            var mapping = new Dictionary<string, string>
            {
                // Consonants
                { "θ", "th" },      // think
                { "ð", "th" },      // this (voiced)
                { "ʃ", "sh" },      // ship
                { "ʒ", "zh" },      // measure
                { "tʃ", "ch" },     // church
                { "dʒ", "j" },      // judge
                { "ŋ", "ng" },      // sing
                { "j", "y" },       // yes
                
                // R-colored vowels
                { "ɜr", "er" },     // bird
                { "ɝ", "er" },      // bird (alternative)
                { "ɑr", "ar" },     // car
                { "ɔr", "or" },     // door
                { "ɪr", "ir" },     // ear
                { "ɛr", "air" },    // care
                { "ʊr", "oor" },    // tour
                
                // Diphthongs
                { "eɪ", "ay" },     // day
                { "aɪ", "i" },      // my
                { "ɔɪ", "oy" },     // boy
                { "aʊ", "ow" },     // how
                { "oʊ", "o" },      // go
                { "ɪə", "ear" },    // here
                { "ɛə", "air" },    // care
                { "ʊə", "oor" },    // tour
                
                // Vowels
                { "ə", "uh" },      // about (schwa)
                { "ʌ", "u" },       // cup
                { "æ", "a" },       // cat
                { "ɑ", "ah" },      // father
                { "ɔ", "aw" },      // law
                { "ɛ", "e" },       // bed
                { "ɪ", "i" },       // sit
                { "i", "ee" },      // see
                { "ʊ", "u" },       // book
                { "u", "oo" },      // food
                
                // Common consonant clusters
                { "str", "str" },
                { "spr", "spr" },
                { "skr", "scr" }
            };

            return mapping.TryGetValue(ipaPhoneme, out var display) ? display : ipaPhoneme;
        }
    }
}

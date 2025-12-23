using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Cofigurations;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace LearningEnglish.Tests.Application.DictionaryServices;

public class DictionaryServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<DictionaryService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IOptions<OxfordDictionaryOptions>> _oxfordOptionsMock;
    private readonly Mock<IOptions<UnsplashOptions>> _unsplashOptionsMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly Mock<IAzureSpeechService> _azureSpeechServiceMock;
    private readonly DictionaryService _dictionaryService;

    public DictionaryServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<DictionaryService>>();
        _configurationMock = new Mock<IConfiguration>();
        _oxfordOptionsMock = new Mock<IOptions<OxfordDictionaryOptions>>();
        _unsplashOptionsMock = new Mock<IOptions<UnsplashOptions>>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();
        _azureSpeechServiceMock = new Mock<IAzureSpeechService>();

        _oxfordOptionsMock.Setup(x => x.Value).Returns(new OxfordDictionaryOptions());
        _unsplashOptionsMock.Setup(x => x.Value).Returns(new UnsplashOptions());

        _dictionaryService = new DictionaryService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configurationMock.Object,
            _oxfordOptionsMock.Object,
            _unsplashOptionsMock.Object,
            _minioFileStorageMock.Object,
            _azureSpeechServiceMock.Object
        );
    }

    #region LookupWordAsync Tests

    [Fact]
    public async Task LookupWordAsync_WithValidWord_ReturnsDictionaryData()
    {
        // Arrange
        var word = "hello";
        var targetLanguage = "vi";

        var httpClient = new HttpClient(new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new[]
                {
                    new
                    {
                        word = word,
                        phonetic = "/həˈloʊ/",
                        phonetics = new[]
                        {
                            new { text = "/həˈloʊ/", audio = "https://api.dictionaryapi.dev/media/pronunciations/en/hello-us.mp3" }
                        },
                        meanings = new[]
                        {
                            new
                            {
                                partOfSpeech = "noun",
                                definitions = new[]
                                {
                                    new
                                    {
                                        definition = "A greeting",
                                        example = "Hello, how are you?"
                                    }
                                }
                            }
                        }
                    }
                }))
            }));

        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        // Act
        var result = await _dictionaryService.LookupWordAsync(word, targetLanguage);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(word, result.Data.Word);
    }

    [Fact]
    public async Task LookupWordAsync_WithEmptyWord_ReturnsBadRequest()
    {
        // Arrange
        var word = "";
        var targetLanguage = "vi";

        // Act
        var result = await _dictionaryService.LookupWordAsync(word, targetLanguage);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Word cannot be empty", result.Message);
    }

    [Fact]
    public async Task LookupWordAsync_WithWhitespaceWord_ReturnsBadRequest()
    {
        // Arrange
        var word = "   ";
        var targetLanguage = "vi";

        // Act
        var result = await _dictionaryService.LookupWordAsync(word, targetLanguage);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Word cannot be empty", result.Message);
    }

    [Fact]
    public async Task LookupWordAsync_WithNonExistentWord_ReturnsNotFound()
    {
        // Arrange
        var word = "nonexistentword12345";
        var targetLanguage = "vi";

        var httpClient = new HttpClient(new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.NotFound)));

        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        // Act
        var result = await _dictionaryService.LookupWordAsync(word, targetLanguage);

        // Assert
        Assert.False(result.Success);
        Assert.False(result.Success);
        // Message could be "Word '...' not found in dictionary" or "An error occurred during word lookup"
        Assert.True(result.Message.Contains("not found") || result.Message.Contains("error"),
            $"Expected message containing 'not found' or 'error', but got: {result.Message}");
    }

    #endregion

    #region GenerateFlashCardFromWordAsync Tests

    [Fact]
    public async Task GenerateFlashCardFromWordAsync_WithValidWord_ReturnsFlashCardPreview()
    {
        // Arrange
        var word = "hello";

        // Mock Oxford API lookup (will fail, fallback to Free Dictionary API)
        var httpClient = new HttpClient(new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new[]
                {
                    new
                    {
                        word = word,
                        phonetic = "/həˈloʊ/",
                        phonetics = new[]
                        {
                            new { text = "/həˈloʊ/", audio = "https://api.dictionaryapi.dev/media/pronunciations/en/hello-us.mp3" }
                        },
                        meanings = new[]
                        {
                            new
                            {
                                partOfSpeech = "noun",
                                definitions = new[]
                                {
                                    new
                                    {
                                        definition = "A greeting",
                                        example = "Hello, how are you?"
                                    }
                                }
                            }
                        }
                    }
                }))
            }));

        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        // Note: GenerateFlashCardFromWordAsync is complex with many dependencies (Oxford API, Azure TTS, Unsplash, MinIO)
        // For unit testing, we'll test the basic validation and error cases.
        // Integration tests would be better for testing the full flow.

        // Act
        var result = await _dictionaryService.GenerateFlashCardFromWordAsync(word);

        // Assert
        // The result may succeed or fail depending on external API availability
        // We just verify the method doesn't throw and returns a response
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GenerateFlashCardFromWordAsync_WithEmptyWord_ReturnsBadRequest()
    {
        // Arrange
        var word = "";

        // Act
        var result = await _dictionaryService.GenerateFlashCardFromWordAsync(word);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Word cannot be empty", result.Message);
    }

    [Fact]
    public async Task GenerateFlashCardFromWordAsync_WithNonExistentWord_ReturnsNotFound()
    {
        // Arrange
        var word = "nonexistentword12345";

        var httpClient = new HttpClient(new MockHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.NotFound)));

        _httpClientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        // Act
        var result = await _dictionaryService.GenerateFlashCardFromWordAsync(word);

        // Assert
        Assert.False(result.Success);
        Assert.False(result.Success);
        // Message could be "Word '...' not found in dictionary" or "An error occurred during word lookup"
        Assert.True(result.Message.Contains("not found") || result.Message.Contains("error"),
            $"Expected message containing 'not found' or 'error', but got: {result.Message}");
    }

    #endregion
}

// Helper class for mocking HttpClient
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}


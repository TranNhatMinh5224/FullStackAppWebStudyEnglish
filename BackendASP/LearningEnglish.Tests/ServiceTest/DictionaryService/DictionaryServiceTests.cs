using System.Net;
using System.Text.Json;
using LearningEnglish.Application.Cofigurations;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.DictionaryService;

public class DictionaryServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<LearningEnglish.Application.Service.DictionaryService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IMinioFileStorage> _minioServiceMock;
    private readonly Mock<IAzureSpeechService> _azureSpeechServiceMock;
    private readonly OxfordDictionaryOptions _oxfordOptions;
    private readonly UnsplashOptions _unsplashOptions;
    private readonly LearningEnglish.Application.Service.DictionaryService _service;

    public DictionaryServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<LearningEnglish.Application.Service.DictionaryService>>();
        _configurationMock = new Mock<IConfiguration>();
        _minioServiceMock = new Mock<IMinioFileStorage>();
        _azureSpeechServiceMock = new Mock<IAzureSpeechService>();
        
        _oxfordOptions = new OxfordDictionaryOptions { BaseUrl = "https://oxford.com", AppId = "id", AppKey = "key" };
        _unsplashOptions = new UnsplashOptions { BaseUrl = "https://unsplash.com", AccessKey = "key" };

        var oxfordOptionsMock = Options.Create(_oxfordOptions);
        var unsplashOptionsMock = Options.Create(_unsplashOptions);

        _service = new LearningEnglish.Application.Service.DictionaryService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configurationMock.Object,
            oxfordOptionsMock,
            unsplashOptionsMock,
            _minioServiceMock.Object,
            _azureSpeechServiceMock.Object
        );
    }

    [Fact]
    public async Task LookupWordAsync_OxfordFails_FallbackToFreeDictionary()
    {
        // Arrange
        var word = "hello";
        
        // Setup HttpClient for Oxford (Fail) and FreeDict (Success)
        var handlerMock = new Mock<HttpMessageHandler>();
        
        // Oxford Fail (404)
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("oxford")), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Free Dictionary Success (200)
        var freeDictResponse = new List<object> { new { word = "hello", meanings = new List<object>() } };
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("dictionaryapi")), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(freeDictResponse)) });

        var client = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        // Act
        var result = await _service.LookupWordAsync(word);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("hello", result.Data.Word);
        _loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GenerateFlashCardFromWordAsync_Success_CombinesAllData()
    {
        // Arrange
        var word = "apple";
        
        // Mock LookupWord (Internal logic) via HttpClient
        var handlerMock = new Mock<HttpMessageHandler>();
        var freeDictResponse = new List<object> 
        { 
            new { 
                word = "apple", 
                phonetic = "/ˈap(ə)l/",
                meanings = new List<object> { 
                    new { 
                        partOfSpeech = "noun", 
                        definitions = new List<object> { new { definition = "A fruit", example = "Eat an apple" } },
                        synonyms = new List<string> { "fruit" }
                    } 
                } 
            } 
        };
        
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(freeDictResponse)) });

        var client = new HttpClient(handlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        // Mock Azure TTS
        _azureSpeechServiceMock.Setup(s => s.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new MemoryStream(new byte[10]));
        
        // Mock MinIO Upload
        _minioServiceMock.Setup(s => s.UpLoadFileTempAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ServiceResponse<FileTempResponseDto> { Success = true, Data = new FileTempResponseDto { TempKey = "temp-key" } });

        // Act
        var result = await _service.GenerateFlashCardFromWordAsync(word);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("apple", result.Data.Word);
        Assert.Equal("/ˈap(ə)l/", result.Data.Pronunciation);
        Assert.Equal("temp-key", result.Data.AudioTempKey);
        // Note: Image will be null if Unsplash is not fully mocked, but the primary flow should pass
    }
}

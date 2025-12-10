using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using LearningEnglish.Application.Service.Auth;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Cofigurations;
using LearningEnglish.Application.DTOs;
using AutoMapper;
using System.Net.Http;
using System.Net;
using System.Text;

namespace LearningEnglish.Tests.Auth
{
    public class FacebookLoginServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IExternalLoginRepository> _externalLoginRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<FacebookLoginService>> _loggerMock;
        private readonly Mock<IOptions<FacebookAuthOptions>> _optionsMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IMinioFileStorage> _minioServiceMock;
        private readonly FacebookLoginService _facebookLoginService;

        public FacebookLoginServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _externalLoginRepositoryMock = new Mock<IExternalLoginRepository>();
            _mapperMock = new Mock<IMapper>();
            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<FacebookLoginService>>();
            _optionsMock = new Mock<IOptions<FacebookAuthOptions>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _minioServiceMock = new Mock<IMinioFileStorage>();

            var facebookOptions = new FacebookAuthOptions
            {
                AppId = "206087408172630",
                AppSecret = "da8c3e42f24645fe6b6ea809b150bc2d"
            };

            _optionsMock.Setup(x => x.Value).Returns(facebookOptions);

            _facebookLoginService = new FacebookLoginService(
                _userRepositoryMock.Object,
                _externalLoginRepositoryMock.Object,
                _mapperMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object,
                _optionsMock.Object,
                _httpClientFactoryMock.Object,
                _minioServiceMock.Object
            );
        }

        [Fact]
        public async Task HandleFacebookLoginAsync_MissingState_ShouldReturnBadRequest()
        {
            // Arrange
            var dto = new FacebookLoginDto
            {
                AccessToken = "valid_token",
                State = string.Empty  // Missing state
            };

            // Act
            var result = await _facebookLoginService.HandleFacebookLoginAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("State parameter", result.Message);
        }

        [Fact]
        public async Task HandleFacebookLoginAsync_InvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            var dto = new FacebookLoginDto
            {
                AccessToken = "invalid_token",
                State = "csrf_state_123"
            };

            // Mock HTTP response for token verification failure
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClientMock = new HttpClient(httpMessageHandlerMock.Object);

            httpMessageHandlerMock.Setup(x => x.SendAsync(
                It.IsAny<HttpRequestMessage>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClientMock);

            // Act
            var result = await _facebookLoginService.HandleFacebookLoginAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(401, result.StatusCode);
            Assert.Contains("Token Facebook không hợp lệ", result.Message);
        }

        [Fact]
        public async Task HandleFacebookLoginAsync_ValidToken_NewUser_ShouldCreateUserAndReturnToken()
        {
            // Arrange
            var dto = new FacebookLoginDto
            {
                AccessToken = "valid_facebook_token",
                State = "csrf_state_123"
            };

            // Mock successful Facebook API responses
            var verificationResponse = @"{
                ""data"": {
                    ""is_valid"": true,
                    ""user_id"": ""facebook_user_123""
                }
            }";

            var userInfoResponse = @"{
                ""id"": ""facebook_user_123"",
                ""email"": ""newuser@facebook.com"",
                ""first_name"": ""John"",
                ""last_name"": ""Doe"",
                ""picture"": {
                    ""data"": {
                        ""url"": ""https://graph.facebook.com/123/picture""
                    }
                }
            }";

            // Setup HTTP client mock
            // ... (HTTP client setup for successful responses)

            // Setup repository mocks
            _externalLoginRepositoryMock
                .Setup(x => x.GetByProviderAndUserIdAsync("Facebook", "facebook_user_123"))
                .ReturnsAsync((ExternalLogin)null);

            _userRepositoryMock
                .Setup(x => x.GetUserByEmailAsync("newuser@facebook.com"))
                .ReturnsAsync((User)null);

            // Mock token generation
            _tokenServiceMock
                .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
                .Returns(("access_token_here", DateTime.UtcNow.AddHours(1)));

            _tokenServiceMock
                .Setup(x => x.GenerateRefreshToken(It.IsAny<User>()))
                .Returns(new RefreshToken { Token = "refresh_token_here" });

            // Act
            var result = await _facebookLoginService.HandleFacebookLoginAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.AccessToken);
            Assert.Contains("thành công", result.Message);

            // Verify user creation
            _userRepositoryMock.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Once);
            _externalLoginRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ExternalLogin>()), Times.Once);
        }
    }
}

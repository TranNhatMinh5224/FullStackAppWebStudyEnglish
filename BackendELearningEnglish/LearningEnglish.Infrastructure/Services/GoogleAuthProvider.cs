using Google.Apis.Auth;
using LearningEnglish.Application.Cofigurations;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearningEnglish.Infrastructure.Services
{
    // Google OAuth2 provider - xử lý token exchange và lấy thông tin user từ Google API
    public class GoogleAuthProvider : IGoogleAuthProvider
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleAuthOptions _options;
        private readonly ILogger<GoogleAuthProvider> _logger;

        public GoogleAuthProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<GoogleAuthOptions> options,
            ILogger<GoogleAuthProvider> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _options = options.Value;
            _logger = logger;
        }

        public async Task<GoogleUserInfo?> GetUserInfoFromCodeAsync(string code)
        {
            try
            {
                // Bước 1: Exchange authorization code → access token
                var tokenResponse = await ExchangeCodeForTokenAsync(code);
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
                {
                    _logger.LogError("Failed to exchange code for token with Google");
                    return null;
                }

                // Bước 2: Validate ID token và lấy user info
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(
                        tokenResponse.IdToken,
                        new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[] { _options.ClientId }
                        });
                }
                catch (InvalidJwtException ex)
                {
                    _logger.LogError(ex, "Invalid Google ID token");
                    return null;
                }

                // Bước 3: Map sang GoogleUserInfo
                return new GoogleUserInfo
                {
                    Subject = payload.Subject,
                    Email = payload.Email ?? "",
                    GivenName = payload.GivenName,
                    FamilyName = payload.FamilyName,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    EmailVerified = payload.EmailVerified
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info from Google");
                return null;
            }
        }

        
        private async Task<GoogleTokenResponse?> ExchangeCodeForTokenAsync(string code)
        {
            try
            {
                var tokenEndpoint = "https://oauth2.googleapis.com/token";
                var requestBody = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", _options.ClientId ?? "" },
                    { "client_secret", _options.ClientSecret ?? "" },
                    { "redirect_uri", _options.RedirectUri ?? "" },
                    { "grant_type", "authorization_code" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
                {
                    Content = new FormUrlEncodedContent(requestBody)
                };

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to exchange code for token. Status: {Status}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging authorization code for token");
                return null;
            }
        }

        // Model nhận token response từ Google
        private class GoogleTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;

            [JsonPropertyName("scope")]
            public string Scope { get; set; } = string.Empty;

            [JsonPropertyName("id_token")]
            public string IdToken { get; set; } = string.Empty;

            [JsonPropertyName("refresh_token")]
            public string? RefreshToken { get; set; }
        }
    }
}

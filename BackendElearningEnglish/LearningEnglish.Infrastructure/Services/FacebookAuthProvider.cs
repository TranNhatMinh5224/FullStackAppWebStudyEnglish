using LearningEnglish.Application.Cofigurations;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearningEnglish.Infrastructure.Services
{
    // Facebook OAuth2 provider - xử lý token exchange và lấy thông tin user từ Facebook Graph API
    public class FacebookAuthProvider : IFacebookAuthProvider
    {
        private readonly HttpClient _httpClient;
        private readonly FacebookAuthOptions _options;
        private readonly ILogger<FacebookAuthProvider> _logger;

        public FacebookAuthProvider(
            IHttpClientFactory httpClientFactory,
            IOptions<FacebookAuthOptions> options,
            ILogger<FacebookAuthProvider> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _options = options.Value;
            _logger = logger;
        }

        public async Task<FacebookUserInfo?> GetUserInfoFromCodeAsync(string code)
        {
            try
            {
                // Bước 1: Exchange authorization code → access token
                var accessToken = await ExchangeCodeForTokenAsync(code);
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Failed to exchange code for token with Facebook");
                    return null;
                }

                // Bước 2: Verify token với Facebook
                if (!await VerifyAccessTokenAsync(accessToken))
                {
                    _logger.LogError("Failed to verify access token with Facebook");
                    return null;
                }

                // Bước 3: Lấy thông tin user từ Facebook Graph API
                var userInfoUrl = $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,picture&access_token={accessToken}";
                var userInfoResponse = await _httpClient.GetAsync(userInfoUrl);

                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get user info from Facebook. Status: {Status}",
                        userInfoResponse.StatusCode);
                    return null;
                }

                var userInfoJson = await userInfoResponse.Content.ReadAsStringAsync();
                var facebookUser = JsonSerializer.Deserialize<FacebookGraphUser>(userInfoJson);

                if (facebookUser == null)
                {
                    _logger.LogError("Failed to deserialize Facebook user info");
                    return null;
                }

                // Nếu Facebook không trả về email, tạo email tự động
                var email = facebookUser.Email;
                if (string.IsNullOrEmpty(email))
                {
                    email = $"facebook_{facebookUser.Id}@noemail.local";
                    _logger.LogInformation("Facebook user {UserId} không có email, tạo email tự động: {Email}",
                        facebookUser.Id, email);
                }

                // Bước 4: Map sang FacebookUserInfo
                return new FacebookUserInfo
                {
                    Id = facebookUser.Id,
                    Email = email,
                    FirstName = facebookUser.FirstName,
                    LastName = facebookUser.LastName,
                    PictureUrl = facebookUser.Picture?.Data?.Url
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info from Facebook");
                return null;
            }
        }

        // Exchange authorization code với Facebook để lấy access token
        private async Task<string?> ExchangeCodeForTokenAsync(string code)
        {
            try
            {
                var tokenEndpoint = "https://graph.facebook.com/v18.0/oauth/access_token";
                var requestUrl = $"{tokenEndpoint}?" +
                    $"client_id={_options.AppId}&" +
                    $"client_secret={_options.AppSecret}&" +
                    $"redirect_uri={Uri.EscapeDataString(_options.RedirectUri)}&" +
                    $"code={code}";

                var response = await _httpClient.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to exchange code for token. Status: {Status}, Error: {Error}",
                        response.StatusCode, errorContent);
                    return null;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<FacebookTokenResponse>(jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return tokenResponse?.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging authorization code for token");
                return null;
            }
        }

        // Verify token với Facebook để đảm bảo tính hợp lệ
        private async Task<bool> VerifyAccessTokenAsync(string accessToken)
        {
            try
            {
                var verifyUrl = $"https://graph.facebook.com/debug_token?" +
                    $"input_token={accessToken}&" +
                    $"access_token={_options.AppId}|{_options.AppSecret}";

                var verifyResponse = await _httpClient.GetAsync(verifyUrl);
                if (!verifyResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                var verifyJson = await verifyResponse.Content.ReadAsStringAsync();
                var verifyData = JsonSerializer.Deserialize<FacebookTokenVerification>(verifyJson);

                return verifyData?.Data?.IsValid == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying access token");
                return false;
            }
        }

        // ===== Private Models =====

        // Model nhận token response từ Facebook
        private class FacebookTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = string.Empty;

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = string.Empty;

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }

        private class FacebookTokenVerification
        {
            [JsonPropertyName("data")]
            public TokenData? Data { get; set; }
        }

        private class TokenData
        {
            [JsonPropertyName("is_valid")]
            public bool IsValid { get; set; }

            [JsonPropertyName("user_id")]
            public string? UserId { get; set; }
        }

        private class FacebookGraphUser
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("first_name")]
            public string? FirstName { get; set; }

            [JsonPropertyName("last_name")]
            public string? LastName { get; set; }

            [JsonPropertyName("picture")]
            public PictureData? Picture { get; set; }
        }

        private class PictureData
        {
            [JsonPropertyName("data")]
            public ImageData? Data { get; set; }
        }

        private class ImageData
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }
        }
    }
}

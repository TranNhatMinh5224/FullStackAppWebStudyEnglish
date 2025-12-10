using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Cofigurations;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOSOptions _options;
        private readonly ILogger<PayOSService> _logger;
        private readonly HttpClient _httpClient;

        public PayOSService(
            IOptions<PayOSOptions> options, 
            ILogger<PayOSService> logger, 
            IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("PayOS");
            _httpClient.BaseAddress = new Uri(_options.ApiUrl);
            _httpClient.DefaultRequestHeaders.Add("x-client-id", _options.ClientId);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        }

        public async Task<ServiceResponse<PayOSLinkResponse>> CreatePaymentLinkAsync(
            CreatePayOSLinkRequest request, 
            decimal amount, 
            string productName, 
            string description)
        {
            var response = new ServiceResponse<PayOSLinkResponse>();
            try
            {
                _logger.LogInformation("Creating PayOS payment link for Payment {PaymentId}, Amount: {Amount}",
                    request.PaymentId, amount);

                // Tạo orderCode từ PaymentId và timestamp
                var orderCode = long.Parse($"{DateTime.UtcNow:yyyyMMddHHmmss}{request.PaymentId:D6}");

                // Tạo request body cho PayOS
                var requestBody = new
                {
                    orderCode = orderCode,
                    amount = (int)amount, // PayOS yêu cầu int (VND)
                    description = description,
                    items = new[]
                    {
                        new
                        {
                            name = productName,
                            quantity = 1,
                            price = (int)amount
                        }
                    },
                    returnUrl = _options.ReturnUrl,
                    cancelUrl = _options.CancelUrl
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("PayOS request: {Request}", jsonContent);

                // Gọi PayOS API
                var httpResponse = await _httpClient.PostAsync("/v2/payment-requests", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("PayOS API error: {StatusCode}, {Response}",
                        httpResponse.StatusCode, responseContent);
                    response.Success = false;
                    response.Message = $"PayOS API error: {responseContent}";
                    return response;
                }

                var payosResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // Parse response
                var data = payosResponse.GetProperty("data");
                var checkoutUrl = data.GetProperty("checkoutUrl").GetString();

                response.Data = new PayOSLinkResponse
                {
                    CheckoutUrl = checkoutUrl ?? string.Empty,
                    OrderCode = orderCode.ToString(),
                    PaymentId = request.PaymentId
                };
                response.Success = true;

                _logger.LogInformation("PayOS payment link created successfully: {CheckoutUrl}", checkoutUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment link");
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<PayOSWebhookDto>> GetPaymentInformationAsync(long orderCode)
        {
            var response = new ServiceResponse<PayOSWebhookDto>();
            try
            {
                _logger.LogInformation("Getting PayOS payment information for order {OrderCode}", orderCode);

                var httpResponse = await _httpClient.GetAsync($"/v2/payment-requests/{orderCode}");
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("PayOS API error: {StatusCode}, {Response}",
                        httpResponse.StatusCode, responseContent);
                    response.Success = false;
                    response.Message = $"PayOS API error: {responseContent}";
                    return response;
                }

                var payosData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var code = payosData.GetProperty("code").GetString() ?? "";
                var data = payosData.GetProperty("data");

                response.Data = new PayOSWebhookDto
                {
                    Code = code,
                    OrderCode = data.GetProperty("orderCode").GetInt64(),
                    Desc = payosData.TryGetProperty("desc", out var desc) ? desc.GetString() ?? "" : "",
                    Data = responseContent,
                    Signature = ""
                };
                response.Success = true;

                _logger.LogInformation("PayOS payment information retrieved: Code={Code}, OrderCode={OrderCode}",
                    code, orderCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayOS payment information");
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public Task<bool> VerifyWebhookSignature(string data, string signature)
        {
            try
            {
                // PayOS sử dụng HMAC SHA256 để verify signature
                var keyBytes = Encoding.UTF8.GetBytes(_options.ChecksumKey);
                var dataBytes = Encoding.UTF8.GetBytes(data);
                
                using var hmac = new HMACSHA256(keyBytes);
                var computedSignature = Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToLower();
                
                var isValid = computedSignature == signature.ToLower();
                
                if (!isValid)
                {
                    _logger.LogWarning("Invalid PayOS webhook signature. Expected: {Expected}, Received: {Received}",
                        computedSignature, signature);
                }
                
                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying PayOS webhook signature");
                return Task.FromResult(false);
            }
        }
    }
}

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
            string description,
            long orderCode)
        {
            var response = new ServiceResponse<PayOSLinkResponse>();
            try
            {
                _logger.LogInformation("Creating PayOS payment link for Payment {PaymentId}, OrderCode: {OrderCode}, Amount: {Amount}",
                    request.PaymentId, orderCode, amount);

                if (amount <= 0 || amount > 100000000) // Max 100 triệu VND
                {
                    _logger.LogError("Invalid amount: {Amount}", amount);
                    response.Success = false;
                    response.Message = "Số tiền không hợp lệ";
                    return response;
                }

                var amountInt = (int)Math.Round(amount, MidpointRounding.AwayFromZero);

                // Description: dùng tên dịch vụ đang định mua (PayOS giới hạn <= 9 ký tự)
                var safeDescription = (description ?? "THANHTOAN").Trim();
                if (safeDescription.Length > 9)
                {
                    safeDescription = safeDescription.Substring(0, 9);
                }

                var baseReturnUrl = _options.ReturnUrl?.Trim() ?? "";
                var baseCancelUrl = _options.CancelUrl?.Trim() ?? "";
                
                var returnUrl = string.IsNullOrEmpty(baseReturnUrl) 
                    ? baseReturnUrl 
                    : $"{baseReturnUrl}{(baseReturnUrl.Contains("?") ? "&" : "?")}orderCode={orderCode}";
                var cancelUrl = string.IsNullOrEmpty(baseCancelUrl) 
                    ? baseCancelUrl 
                    : $"{baseCancelUrl}{(baseCancelUrl.Contains("?") ? "&" : "?")}orderCode={orderCode}";

                
                var signData = $"amount={amountInt}&cancelUrl={cancelUrl}&description={safeDescription}&orderCode={orderCode}&returnUrl={returnUrl}";
                var signature = HmacSha256(signData, _options.ChecksumKey);

                var requestBody = new
                {
                    orderCode = orderCode,
                    amount = amountInt,
                    description = safeDescription,
                    returnUrl = returnUrl,
                    cancelUrl = cancelUrl,
                    signature = signature
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("PayOS request body: {Body}", jsonContent);
                _logger.LogInformation("PayOS signData: {SignData}", signData);
                _logger.LogInformation("PayOS signature: {Signature}", signature);

                // Gọi PayOS API
                var httpResponse = await _httpClient.PostAsync("/v2/payment-requests", content);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

              
                _logger.LogInformation("PayOS API response: StatusCode={StatusCode}, Response={Response}", 
                    httpResponse.StatusCode, responseContent);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("PayOS API error: {StatusCode}, {Response}",
                        httpResponse.StatusCode, responseContent);
                    response.Success = false;
                    response.Message = $"PayOS API error: {responseContent}";
                    return response;
                }

                var payosResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                
                if (payosResponse.TryGetProperty("code", out var codeElement))
                {
                    var code = codeElement.GetString();
                    if (code != "00")
                    {
                        var desc = payosResponse.TryGetProperty("desc", out var descElement)
                            ? descElement.GetString()
                            : "Unknown PayOS error";

                        _logger.LogError("PayOS error: Code={Code}, Desc={Desc}, Response={Response}", 
                            code, desc, responseContent);
                        response.Success = false;
                        response.Message = $"PayOS error: {desc}";
                        return response;
                    }
                }

               
                if (!payosResponse.TryGetProperty("data", out var dataElement) || 
                    dataElement.ValueKind == JsonValueKind.Null)
                {
                    _logger.LogError("PayOS response missing or null 'data' property. Response: {Response}", responseContent);
                    response.Success = false;
                    response.Message = "PayOS response không hợp lệ: thiếu dữ liệu";
                    return response;
                }

                
                if (!dataElement.TryGetProperty("checkoutUrl", out var checkoutUrlElement))
                {
                    _logger.LogError("PayOS response missing 'checkoutUrl' property. Response: {Response}", responseContent);
                    response.Success = false;
                    response.Message = "PayOS response không hợp lệ: thiếu checkoutUrl";
                    return response;
                }

                var checkoutUrl = checkoutUrlElement.GetString();

            
                if (string.IsNullOrWhiteSpace(checkoutUrl))
                {
                    _logger.LogError("PayOS returned empty checkoutUrl. Response: {Response}", responseContent);
                    response.Success = false;
                    response.Message = "PayOS trả checkoutUrl rỗng";
                    return response;
                }

                response.Data = new PayOSLinkResponse
                {
                    CheckoutUrl = checkoutUrl,
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

                var status = data.TryGetProperty("status", out var statusElement) 
                    ? statusElement.GetString() ?? "" 
                    : "";

                response.Data = new PayOSWebhookDto
                {
                    Code = code,
                    OrderCode = data.GetProperty("orderCode").GetInt64(),
                    Desc = payosData.TryGetProperty("desc", out var desc) ? desc.GetString() ?? "" : "",
                    Data = responseContent,
                    Signature = "",
                    Status = status
                };
                response.Success = true;

                _logger.LogInformation("PayOS payment information retrieved: Code={Code}, OrderCode={OrderCode}, Status={Status}",
                    code, orderCode, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayOS payment information");
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        // ✅ Helper method để tạo HMAC SHA256
        private string HmacSha256(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }

        public Task<bool> VerifyWebhookSignature(string data, string signature)
        {
            try
            {
                // PayOS sử dụng HMAC SHA256 để verify signature
                var computedSignature = HmacSha256(data, _options.ChecksumKey);
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

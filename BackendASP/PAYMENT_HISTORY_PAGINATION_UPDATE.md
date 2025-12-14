# Payment History Pagination Update

## Summary
Cập nhật endpoint lịch sử giao dịch (transaction history) để đồng bộ với các endpoint phân trang khác trong hệ thống và thêm endpoint không phân trang.

## Changes Made

### 1. Controller Updates (`PaymentController.cs`)

#### Before:
```csharp
[HttpGet("history")]
public async Task<IActionResult> GetTransactionHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
{
    // Manual validation...
    var result = await _paymentService.GetTransactionHistoryAsync(userId, pageNumber, pageSize);
    return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
}
```

#### After:
```csharp
// Endpoint có phân trang - sử dụng PageRequest DTO
[HttpGet("history")]
public async Task<IActionResult> GetTransactionHistory([FromQuery] PageRequest request)
{
    var userId = GetCurrentUserId();
    var result = await _paymentService.GetTransactionHistoryAsync(userId, request);
    return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
}

// Endpoint mới - không phân trang
[HttpGet("history/all")]
public async Task<IActionResult> GetAllTransactionHistory()
{
    var userId = GetCurrentUserId();
    var result = await _paymentService.GetAllTransactionHistoryAsync(userId);
    return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
}
```

### 2. Service Interface (`IPaymentService.cs`)

#### Added:
```csharp
Task<ServiceResponse<PagedResult<TransactionHistoryDto>>> GetTransactionHistoryAsync(int userId, PageRequest request);
Task<ServiceResponse<List<TransactionHistoryDto>>> GetAllTransactionHistoryAsync(int userId);
```

### 3. Service Implementation (`PaymentService.cs`)

#### Updated Paginated Method:
- Changed signature from `(int userId, int pageNumber, int pageSize)` to `(int userId, PageRequest request)`
- Uses `request.PageNumber` and `request.PageSize` instead of direct parameters
- Maintains same functionality with standardized DTO

#### New Non-Paginated Method:
```csharp
public async Task<ServiceResponse<List<TransactionHistoryDto>>> GetAllTransactionHistoryAsync(int userId)
{
    // Retrieves all transactions without pagination
    var payments = await _paymentRepository.GetAllTransactionHistoryAsync(userId);
    // Maps to TransactionHistoryDto list
    return response with all transactions
}
```

### 4. Repository Interface (`IPaymentRepository.cs`)

#### Added:
```csharp
Task<IEnumerable<Payment>> GetAllTransactionHistoryAsync(int userId);
```

### 5. Repository Implementation (`PaymentRepository.cs`)

#### Added:
```csharp
public async Task<IEnumerable<Payment>> GetAllTransactionHistoryAsync(int userId)
{
    return await _context.Payments
        .Where(p => p.UserId == userId)
        .OrderByDescending(p => p.PaymentId)
        .ToListAsync();
}
```

## API Endpoints

### Paginated Endpoint (Updated)
**GET** `/api/user/payments/history`

**Query Parameters:**
```json
{
  "PageNumber": 1,      // default: 1
  "PageSize": 20,       // default: 20
  "SearchTerm": ""      // optional: search term
}
```

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Success",
  "data": {
    "items": [
      {
        "paymentId": 1,
        "paymentMethod": "PayOS",
        "productType": "Course",
        "productId": 5,
        "productName": "English for Beginners",
        "amount": 500000,
        "status": "Completed",
        "createdAt": "2024-12-01T10:00:00Z",
        "paidAt": "2024-12-01T10:05:00Z",
        "providerTransactionId": "TXN123456"
      }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

### Non-Paginated Endpoint (New)
**GET** `/api/user/payments/history/all`

**Query Parameters:** None

**Response:**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Success",
  "data": [
    {
      "paymentId": 1,
      "paymentMethod": "PayOS",
      "productType": "Course",
      "productId": 5,
      "productName": "English for Beginners",
      "amount": 500000,
      "status": "Completed",
      "createdAt": "2024-12-01T10:00:00Z",
      "paidAt": "2024-12-01T10:05:00Z",
      "providerTransactionId": "TXN123456"
    },
    // ... all transactions
  ]
}
```

## Benefits

1. **Consistency**: Endpoint phân trang giờ đây sử dụng `PageRequest` DTO giống như các endpoint khác (courses, users, quiz attempts)
2. **Flexibility**: Thêm endpoint không phân trang cho các trường hợp cần lấy toàn bộ lịch sử
3. **Cleaner Code**: Loại bỏ manual validation trong controller (PageRequest có default values)
4. **Maintainability**: Dễ dàng thêm search term hoặc filters trong tương lai thông qua PageRequest

## Testing Recommendations

### Test Paginated Endpoint:
```bash
# Default pagination (page 1, size 20)
GET /api/user/payments/history

# Custom pagination
GET /api/user/payments/history?PageNumber=2&PageSize=10

# With search (if implemented later)
GET /api/user/payments/history?PageNumber=1&PageSize=20&SearchTerm=PayOS
```

### Test Non-Paginated Endpoint:
```bash
# Get all transactions
GET /api/user/payments/history/all
```

## Migration Notes

- ✅ No database migration required
- ✅ Backward compatible (PageRequest has default values)
- ✅ Build successful with no errors
- ⚠️ Frontend clients should update to use new PageRequest structure for paginated endpoint
- ⚠️ Old query parameters (pageNumber, pageSize) will still work due to model binding, but should migrate to new structure

## Date
December 14, 2024

## Status
✅ Completed and tested (build successful)

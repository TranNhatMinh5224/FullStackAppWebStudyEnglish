# TỔNG HỢP API THANH TOÁN - MUA KHÓA HỌC & TEACHER PACKAGE

## 📋 DANH SÁCH ENDPOINTS

### 1. TẠO YÊU CẦU THANH TOÁN
**Endpoint:** `POST /api/user/payments/process`  
**Role:** Student  
**Authorization:** `[Authorize(Roles = "Student")]`

#### Input (requestPayment):
```json
{
  "productId": 1,                    // ID khóa học hoặc teacher package
  "typeproduct": 0,                  // ProductType: 0=Course, 1=TeacherPackage
  "idempotencyKey": "uuid-string"    // Optional: UUID để prevent duplicate payments
}
```

#### Output (ServiceResponse<CreateInforPayment>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Tạo thanh toán thành công",
  "data": {
    "paymentId": 123,
    "productType": 0,
    "productId": 1,
    "amount": 500000
  }
}
```

**Lưu ý:**
- Nếu `amount = 0` (miễn phí) → tự động confirm và enroll ngay
- Tạo Payment với `Status = Pending`, `OrderCode`, `ExpiredAt = 15 phút`

---

### 2. TẠO LINK THANH TOÁN PAYOS
**Endpoint:** `POST /api/user/payments/payos/create-link/{paymentId}`  
**Role:** Student  
**Authorization:** `[Authorize(Roles = "Student")]`

#### Input:
- Path parameter: `paymentId` (int)

#### Output (ServiceResponse<PayOSLinkResponse>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Tạo link thanh toán thành công",
  "data": {
    "checkoutUrl": "https://pay.payos.vn/web/...",
    "orderCode": "202512261234567890",
    "paymentId": 123
  }
}
```

**Lưu ý:**
- Payment phải có `Status = Pending`
- Payment phải có `OrderCode` và `Gateway = PayOs`
- Payment chưa hết hạn (`ExpiredAt > DateTime.UtcNow`)

---

### 3. XÁC NHẬN THANH TOÁN (MANUAL)
**Endpoint:** `POST /api/user/payments/confirm`  
**Role:** Student  
**Authorization:** `[Authorize(Roles = "Student")]`

#### Input (CompletePayment):
```json
{
  "paymentId": 123,
  "productId": 1,
  "productType": 0,
  "amount": 500000,
  "paymentMethod": "PayOs"
}
```

#### Output (ServiceResponse<bool>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Xác nhận thanh toán thành công",
  "data": true
}
```

**Lưu ý:**
- Validate payment status, amount, productId, productType
- Update payment status → `Completed`
- Gọi `ProcessPostPaymentAsync` để:
  - **Course**: Tự động enroll vào khóa học
  - **TeacherPackage**: Nâng cấp role Teacher + tạo subscription

---

### 4. XÁC NHẬN THANH TOÁN PAYOS (MANUAL)
**Endpoint:** `POST /api/user/payments/payos/confirm/{paymentId}`  
**Role:** Student  
**Authorization:** `[Authorize(Roles = "Student")]`

#### Input:
- Path parameter: `paymentId` (int)

#### Output (ServiceResponse<bool>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Xác nhận thanh toán thành công",
  "data": true
}
```

**Lưu ý:**
- Kiểm tra payment status trên PayOS trước khi confirm
- Tương tự endpoint `/confirm` nhưng có thêm validation từ PayOS

---

### 5. LẤY LỊCH SỬ GIAO DỊCH (PHÂN TRANG)
**Endpoint:** `GET /api/user/payments/history`  
**Role:** Student  
**Authorization:** `[Authorize(Roles = "Student")]`

#### Input (Query Parameters - PageRequest):
```
?pageNumber=1&pageSize=20
```

#### Output (ServiceResponse<PagedResult<TransactionHistoryDto>>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy lịch sử giao dịch thành công",
  "data": {
    "items": [
      {
        "paymentId": 123,
        "paymentMethod": "PayOs",
        "productType": 0,
        "productId": 1,
        "productName": "Khóa học tiếng Anh cơ bản",
        "amount": 500000,
        "status": 2,
        "createdAt": "2025-12-26T10:00:00Z",
        "paidAt": "2025-12-26T10:05:00Z",
        "providerTransactionId": "202512261234567890"
      }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

**Lưu ý:**
- Sắp xếp theo `PaidAt DESC` (mới nhất lên đầu)
- RLS đã filter theo userId tự động

---

### 6. LẤY CHI TIẾT GIAO DỊCH
**Endpoint:** `GET /api/user/payments/transaction/{paymentId}`  
**Role:** Student  
**Authorization:** `[Authorize(Roles = "Student")]`

#### Input:
- Path parameter: `paymentId` (int)

#### Output (ServiceResponse<TransactionDetailDto>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy chi tiết giao dịch thành công",
  "data": {
    "paymentId": 123,
    "userId": 456,
    "userName": "Nguyễn Văn A",
    "userEmail": "user@example.com",
    "paymentMethod": "PayOs",
    "productType": 0,
    "productId": 1,
    "productName": "Khóa học tiếng Anh cơ bản",
    "amount": 500000,
    "status": 2,
    "createdAt": "2025-12-26T10:00:00Z",
    "paidAt": "2025-12-26T10:05:00Z",
    "providerTransactionId": "202512261234567890"
  }
}
```

**Lưu ý:**
- RLS đã filter theo userId tự động
- Nếu payment không tồn tại hoặc không thuộc về user → 404

---

### 7. PAYOS RETURN URL (REDIRECT)
**Endpoint:** `GET /api/user/payments/payos/return`  
**Role:** AllowAnonymous  
**Authorization:** `[AllowAnonymous]`

#### Input (Query Parameters):
```
?code=00&desc=Success&data={"orderCode":1234567890}
```

#### Output:
- Redirect đến frontend: `/payment-success?paymentId=123&orderCode=1234567890`
- Hoặc: `/payment-failed?reason=...`

**Lưu ý:**
- Tự động confirm payment nếu `Status = Pending`
- RLS compatible (webhook policy cho phép khi `current_user_id IS NULL`)

---

### 8. PAYOS WEBHOOK (CALLBACK)
**Endpoint:** `POST /api/user/payments/payos/webhook`  
**Role:** AllowAnonymous  
**Authorization:** `[AllowAnonymous]`

#### Input (PayOSWebhookDto):
```json
{
  "code": "00",
  "orderCode": 1234567890,
  "desc": "Success",
  "data": "{...}",
  "signature": "abc123..."
}
```

#### Output:
```json
{
  "message": "Success",
  "paymentId": 123
}
```

**Lưu ý:**
- Verify signature trước khi xử lý (HMAC SHA256)
- Tự động confirm payment nếu `Status = Pending`
- RLS compatible (webhook policy cho phép khi `current_user_id IS NULL`)

---

## 🔄 QUY TRÌNH THANH TOÁN HOÀN CHỈNH

### A. MUA KHÓA HỌC (COURSE)

1. **Student tạo payment request**
   ```
   POST /api/user/payments/process
   {
     "productId": 1,
     "typeproduct": 0,
     "idempotencyKey": "uuid"
   }
   → Trả về: PaymentId, Amount
   ```

2. **Nếu amount = 0 (miễn phí)**
   - Tự động confirm ngay
   - Tự động enroll vào khóa học
   - Trả về success

3. **Nếu amount > 0**
   - **Tạo PayOS link:**
     ```
     POST /api/user/payments/payos/create-link/{paymentId}
     → Trả về: CheckoutUrl
     ```
   - **Student thanh toán trên PayOS**
   - **PayOS redirect về Return URL:**
     ```
     GET /api/user/payments/payos/return?code=00&data={...}
     → Redirect: /payment-success
     ```
   - **PayOS gửi Webhook:**
     ```
     POST /api/user/payments/payos/webhook
     → Tự động confirm payment
     ```
   - **Sau khi confirm:**
     - Update payment status → `Completed`
     - Tự động enroll vào khóa học (CoursePaymentProcessor)
     - Tạo notification thành công

---

### B. MUA TEACHER PACKAGE

1. **Student tạo payment request**
   ```
   POST /api/user/payments/process
   {
     "productId": 1,
     "typeproduct": 1,
     "idempotencyKey": "uuid"
   }
   → Trả về: PaymentId, Amount
   ```

2. **Tạo PayOS link và thanh toán** (tương tự Course)

3. **Sau khi confirm:**
   - Update payment status → `Completed`
   - Nâng cấp role User → Teacher (TeacherPackagePaymentProcessor)
   - Tạo TeacherSubscription
   - Tạo notification thành công

---

## 📊 DTOs

### requestPayment
```csharp
public class requestPayment
{
    public int ProductId { get; set; }
    public ProductType typeproduct { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}
```

### CreateInforPayment
```csharp
public class CreateInforPayment
{
    public int PaymentId { get; set; }
    public ProductType ProductType { get; set; }
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
}
```

### CompletePayment
```csharp
public class CompletePayment
{
    public int PaymentId { get; set; }
    public int ProductId { get; set; }
    public ProductType ProductType { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}
```

### TransactionHistoryDto
```csharp
public class TransactionHistoryDto
{
    public int PaymentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ProviderTransactionId { get; set; }
}
```

### TransactionDetailDto
```csharp
public class TransactionDetailDto
{
    public int PaymentId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ProviderTransactionId { get; set; }
}
```

### PayOSLinkResponse
```csharp
public class PayOSLinkResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public int PaymentId { get; set; }
}
```

### PayOSWebhookDto
```csharp
public class PayOSWebhookDto
{
    public string Code { get; set; } = string.Empty; // "00" = thành công
    public long OrderCode { get; set; }
    public string Desc { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
```

---

## ✅ VALIDATION

### RequestPaymentValidator
- `ProductId > 0`
- `typeproduct` phải là enum hợp lệ
- `IdempotencyKey` tối đa 100 ký tự (nếu có)

### CompletePaymentValidator
- `PaymentId > 0`
- `ProductId > 0`
- `ProductType` phải là enum hợp lệ
- `Amount > 0`
- `PaymentMethod` không rỗng, tối đa 50 ký tự

---

## 🔒 SECURITY & RLS

- **Student endpoints**: RLS tự động filter theo `UserId = app.current_user_id()`
- **Webhook/Return URL**: RLS policy cho phép khi `current_user_id IS NULL`
- **Idempotency Key**: Prevent duplicate payments
- **Webhook Signature**: Verify HMAC SHA256

---

## 🗑️ FILES/INTERFACES ĐÃ XÓA

1. **Endpoint duplicate**: `GET /api/user/payments/history/all` (đã xóa, trùng với `/history`)

---

## 📝 NOTES

- Tất cả endpoints đã được chuẩn hóa
- RLS đã được implement đầy đủ
- Validation đầy đủ với FluentValidation
- Error handling đầy đủ
- Logging chi tiết


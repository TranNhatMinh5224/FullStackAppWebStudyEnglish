# 🔧 FIX CHỨC NĂNG QUÊN MẬT KHẨU

## 📋 FLOW ĐÚNG (3 Endpoints)

```
1. POST /api/auth/forgot-password (email)
   ↓ Generate OTP 6 số, gửi email
   ↓ IsUsed = false, AttemptsCount = 0
   
2. POST /api/auth/verify-otp (email, otpCode)
   ↓ Verify OTP
   ↓ Nếu đúng: Mark IsUsed = true (KHÔNG XÓA)
   ↓ Nếu sai ≥5 lần: XÓA token
   
3. POST /api/auth/set-new-password (email, otpCode, newPassword)
   ↓ Check IsUsed = true (đã verify)
   ↓ Set password mới
   ↓ XÓA token
```

---

## ✅ CÁC VẤN ĐỀ ĐÃ FIX

### **1. ❌ BUG: VerifyOtpAsync XÓA TOKEN**

**Trước:**
```csharp
public async Task<ServiceResponse<bool>> VerifyOtpAsync(VerifyOtpDto dto)
{
    // ...verify OTP...
    
    // ❌ OTP đúng: XÓA token ngay!
    await _passwordResetTokenRepository.DeleteAsync(otpToken);
    
    return response; // "Xác thực thành công"
}
```

**Sau:**
```csharp
public async Task<ServiceResponse<bool>> VerifyOtpAsync(VerifyOtpDto dto)
{
    // ...verify OTP...
    
    // ✅ OTP đúng: MARK IsUsed = true (KHÔNG XÓA)
    otpToken.IsUsed = true;
    await _passwordResetTokenRepository.UpdateAsync(otpToken);
    
    return response; // "Xác thực thành công"
}
```

---

### **2. ❌ BUG: SetNewPasswordAsync KHÔNG THỂ TÌM TOKEN**

**Trước:**
```csharp
public async Task<ServiceResponse<bool>> SetNewPasswordAsync(SetNewPasswordDto dto)
{
    // ❌ Tìm token với IsUsed = false
    var otpToken = await _passwordResetTokenRepository.GetActiveTokenByUserIdAsync(user.UserId);
    
    if (otpToken == null)  // ← LUÔN NULL vì IsUsed = true!
    {
        response.Message = "Mã OTP không hợp lệ";
        return response;
    }
}
```

**Sau:**
```csharp
public async Task<ServiceResponse<bool>> SetNewPasswordAsync(SetNewPasswordDto dto)
{
    // ✅ Tìm token theo OTP code (không filter IsUsed)
    var otpToken = await _passwordResetTokenRepository.GetByTokenAsync(dto.OtpCode);
    
    if (otpToken == null || otpToken.UserId != user.UserId)
    {
        response.Message = "Mã OTP không hợp lệ";
        return response;
    }
    
    // ✅ Check IsUsed = true (đã verify)
    if (!otpToken.IsUsed)
    {
        response.Message = "Vui lòng xác thực mã OTP trước";
        return response;
    }
    
    // ✅ Set password
    user.SetPassword(dto.NewPassword);
    await _userRepository.UpdateUserAsync(user);
    
    // ✅ XÓA token sau khi set password thành công
    await _passwordResetTokenRepository.DeleteAsync(otpToken);
}
```

---

### **3. ❌ BUG: Logic AttemptsCount SAI**

**Trước:**
```csharp
// Nếu nhập sai >= 5 lần, khóa 20 phút
if (otpToken.AttemptsCount >= 5)
{
    await _passwordResetTokenRepository.DeleteAsync(otpToken);
    response.Message = "Bạn đã nhập sai OTP quá 5 lần. Tài khoản bị khóa trong 20 phút";
    return response;
}

// Nếu nhập sai >= 10 lần...
if (otpToken.AttemptsCount >= 10)  // ← KHÔNG BAO GIỜ ĐẾN ĐÂY!
{
    await _passwordResetTokenRepository.DeleteAsync(otpToken);
}
```

**Sau:**
```csharp
// Nếu nhập sai >= 5 lần, XÓA token
if (otpToken.AttemptsCount >= 5)
{
    await _passwordResetTokenRepository.DeleteAsync(otpToken);
    response.Message = "Bạn đã nhập sai OTP quá 5 lần. Vui lòng yêu cầu mã OTP mới";
    return response;
}
// ← XÓA logic >= 10 lần (không cần thiết)
```

---

### **4. ❌ BUG: Random KHÔNG AN TOÀN**

**Trước:**
```csharp
// ❌ KHÔNG cryptographically secure
var random = new Random();
var otpCode = random.Next(100000, 999999).ToString();
```

**Sau:**
```csharp
// ✅ Cryptographically secure
using System.Security.Cryptography;

var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
```

**Áp dụng cho:**
- ✅ `PasswordService.cs` → `ForgotPasswordAsync()`
- ✅ `RegisterService.cs` → `RegisterUserAsync()`

---

## 📝 CHECKLIST

- [x] **VerifyOtpAsync**: Mark `IsUsed = true` thay vì XÓA
- [x] **SetNewPasswordAsync**: Tìm token theo OTP code, check IsUsed, XÓA sau khi set password
- [x] **AttemptsCount**: Xóa logic >= 10 lần (không cần thiết)
- [x] **Random → RandomNumberGenerator**: Thay ở PasswordService + RegisterService
- [x] **Import using**: Thêm `using System.Security.Cryptography;`

---

## 🎯 KẾT QUẢ

### **Flow hoàn chỉnh:**

1. **User quên mật khẩu** → Gọi `/forgot-password`
   - System tạo OTP, gửi email
   - Token: `IsUsed = false`, `AttemptsCount = 0`

2. **User nhập OTP** → Gọi `/verify-otp`
   - Nếu đúng: `IsUsed = true` → Response: "Xác thực thành công"
   - Nếu sai: `AttemptsCount++`
   - Nếu sai ≥5 lần: XÓA token → "Vui lòng yêu cầu OTP mới"

3. **User nhập password mới** → Gọi `/set-new-password`
   - Tìm token theo OTP code
   - Check `IsUsed = true` (đã verify)
   - Set password mới
   - XÓA token

### **Bảo mật:**
- ✅ OTP cryptographically secure
- ✅ Rate limiting (3 lần/20 phút, cách 1 phút)
- ✅ Brute-force protection (5 lần thử sai)
- ✅ OTP hết hạn sau 5 phút
- ✅ Token bị xóa sau khi hoàn thành

---

## 🚀 NEXT STEPS (Optional)

1. **Thêm Rate Limiting Middleware** cho các endpoints:
   ```csharp
   [EnableRateLimiting("forgot-password")]  // 3 lần/phút/IP
   [EnableRateLimiting("verify-otp")]        // 10 lần/phút/IP
   ```

2. **Hash OTP trong database** (optional):
   ```csharp
   var hashedOtp = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(otpCode)));
   ```

3. **Background Job xóa expired tokens** (optional):
   - Chạy mỗi 1 giờ để cleanup tokens hết hạn

4. **Logging & Monitoring**:
   - Log failed attempts
   - Alert khi có suspicious activity (nhiều IPs thử OTP)

# 🔍 HƯỚNG DẪN DEBUG VẤN ĐỀ ĐĂNG NHẬP

## 🐛 Vấn đề
Đăng nhập thành công nhưng hiển thị sai tài khoản (có thể là tài khoản cũ từ localStorage).

## ✅ Đã sửa

### 1. **Response Format Handling**
- ✅ Sửa `authService.js` để unwrap nested response data
- ✅ Backend trả về: `{ success: true, data: { accessToken, refreshToken, user, expiresAt } }`
- ✅ httpClient wrap lại: `{ success: true, data: { success: true, data: {...} } }`
- ✅ Đã sửa để unwrap đúng: `result.data.data || result.data`

### 2. **Role Extraction từ JWT**
- ✅ Sửa `jwtUtils.js` để extract roles đúng từ ClaimTypes.Role
- ✅ Backend dùng: `ClaimTypes.Role` = `"http://schemas.microsoft.com/ws/2008/06/identity/claims/role"`
- ✅ Đã thêm logging để debug

### 3. **User Data Validation**
- ✅ Thêm validation để đảm bảo user data từ response khớp với token
- ✅ Kiểm tra userId từ token vs userId từ response

### 4. **Logging**
- ✅ Thêm console.log ở tất cả các bước để debug:
  - `[AuthService]` - Log trong authService
  - `[AuthContext]` - Log trong AuthContext
  - `[httpClient]` - Log trong httpClient
  - `[jwtUtils]` - Log trong jwtUtils

## 🔍 Cách Debug

### Bước 1: Mở Browser Console
1. Mở DevTools (F12)
2. Vào tab Console
3. Clear console

### Bước 2: Đăng nhập
1. Nhập email và password
2. Xem các log trong console:
   - `[AuthService] Login request:` - Email được gửi
   - `[httpClient] HTTP Request:` - Request details
   - `[httpClient] Response data structure:` - Response structure
   - `[AuthService] Login response:` - Response data
   - `[AuthContext] Login result:` - Result trong context
   - `[jwtUtils] Decoded token:` - Token decoded
   - `[jwtUtils] Extracted roles:` - Roles extracted

### Bước 3: Kiểm tra các điểm

#### ✅ Kiểm tra Response Structure
```javascript
// Trong console, kiểm tra:
// 1. Response có đúng structure không?
[AuthService] Login response: {
  success: true,
  hasData: true,
  hasUser: true,
  user: { userId: X, email: "...", ... }
}
```

#### ✅ Kiểm tra Token
```javascript
// 2. Token có được lưu không?
localStorage.getItem('accessToken') // Phải có giá trị

// 3. Token có chứa đúng userId không?
// Decode token và kiểm tra 'sub' claim
```

#### ✅ Kiểm tra User Data
```javascript
// 4. User data có đúng không?
const user = JSON.parse(localStorage.getItem('user'));
console.log('Stored user:', user);
// Phải khớp với email bạn đăng nhập
```

#### ✅ Kiểm tra Roles
```javascript
// 5. Roles có được extract đúng không?
// Xem log [jwtUtils] Extracted roles
```

## 🎯 Các Vấn Đề Có Thể Gặp

### Vấn đề 1: User Data từ localStorage cũ
**Triệu chứng:** Đăng nhập tài khoản A nhưng hiển thị tài khoản B

**Nguyên nhân:** 
- localStorage còn user data từ lần login trước
- AuthContext initialize lấy user từ localStorage trước khi gọi getProfile

**Giải pháp:**
- Clear localStorage trước khi test: `localStorage.clear()`
- Hoặc logout trước khi login lại

### Vấn đề 2: Response nested structure
**Triệu chứng:** `result.data.user` là undefined

**Nguyên nhân:**
- Backend trả về nested: `{ success: true, data: { user: {...} } }`
- httpClient wrap lại: `{ success: true, data: { success: true, data: { user: {...} } } }`
- Cần unwrap: `result.data.data.user`

**Đã sửa:** ✅ authService.js đã unwrap đúng

### Vấn đề 3: Roles không được extract
**Triệu chứng:** Role là null hoặc undefined

**Nguyên nhân:**
- JWT claim name không match
- Backend dùng `ClaimTypes.Role` nhưng frontend tìm sai key

**Đã sửa:** ✅ jwtUtils.js đã tìm đúng claim name

## 🧪 Test Cases

### Test 1: Login với tài khoản mới
1. Clear localStorage: `localStorage.clear()`
2. Đăng nhập với email/password
3. Kiểm tra console logs
4. Kiểm tra localStorage có đúng user không

### Test 2: Login sau khi đã có token cũ
1. Đăng nhập tài khoản A
2. Logout
3. Đăng nhập tài khoản B
4. Kiểm tra user data có đúng tài khoản B không

### Test 3: Refresh page sau khi login
1. Đăng nhập
2. Refresh page (F5)
3. Kiểm tra AuthContext có gọi getProfile và lấy đúng user không

## 📝 Checklist Debug

- [ ] Console có log `[AuthService] Login request` với đúng email?
- [ ] Console có log `[httpClient] HTTP Request` với đúng URL?
- [ ] Console có log `[httpClient] Response data structure`?
- [ ] Console có log `[AuthService] Login response` với user data?
- [ ] Console có log `[AuthContext] Login result`?
- [ ] Console có log `[jwtUtils] Decoded token`?
- [ ] Console có log `[jwtUtils] Extracted roles`?
- [ ] localStorage có `accessToken`?
- [ ] localStorage có `user` với đúng email?
- [ ] Token có chứa đúng userId trong claim `sub`?
- [ ] Roles có được extract đúng?

## 🚀 Next Steps

Nếu vẫn còn vấn đề sau khi có logs:
1. Copy tất cả console logs
2. Kiểm tra:
   - Response structure từ backend
   - Token content (decode JWT)
   - localStorage content
3. So sánh userId từ token vs userId từ response


# SuperAdmin - Tổng hợp Quyền và Chức năng

## 🔐 Quyền của SuperAdmin

SuperAdmin có **toàn quyền** trong hệ thống, tự động pass tất cả các permission checks.

---

## 📋 1. QUẢN LÝ ADMIN (Admin Management)

### 1.1. Tạo Admin mới
- **Endpoint:** `POST /api/superadmin/admins`
- **Mô tả:** Tạo tài khoản admin mới (ContentAdmin hoặc FinanceAdmin)
- **Input:**
  ```json
  {
    "email": "admin1@gmail.com",
    "password": "12345678",
    "firstName": "admin1",
    "lastName": "system",
    "phoneNumber": "0982345678",
    "roleId": 2  // 2 = ContentAdmin, 3 = FinanceAdmin
  }
  ```

### 1.2. Xem danh sách Admins
- **Endpoint:** `GET /api/superadmin/admins`
- **Mô tả:** Lấy danh sách tất cả admins với phân trang
- **Query Parameters:** `PageNumber`, `PageSize`, `SearchTerm`

### 1.3. Xem chi tiết Admin
- **Endpoint:** `GET /api/superadmin/admins/{userId}`
- **Mô tả:** Xem thông tin chi tiết của một admin

### 1.4. Xóa Admin
- **Endpoint:** `DELETE /api/superadmin/admins/{userId}`
- **Mô tả:** Xóa admin (remove Admin role khỏi user)

### 1.5. Reset Password Admin
- **Endpoint:** `PUT /api/superadmin/admins/{userId}/reset-password`
- **Mô tả:** Reset password cho admin
- **Input:**
  ```json
  {
    "newPassword": "newpassword123"
  }
  ```

### 1.6. Đổi Email Admin
- **Endpoint:** `PUT /api/superadmin/admins/{userId}/email`
- **Mô tả:** Đổi email cho admin
- **Input:**
  ```json
  {
    "newEmail": "newemail@gmail.com"
  }
  ```

---

## 👥 2. QUẢN LÝ USER (User Management)

### 2.1. Gán Role cho User
- **Endpoint:** `POST /api/superadmin/users/{userId}/roles`
- **Mô tả:** Gán role cho user (SuperAdmin, ContentAdmin, FinanceAdmin, Teacher, Student)
- **Input:**
  ```json
  {
    "roleName": "Teacher"
  }
  ```

### 2.2. Xóa Role khỏi User
- **Endpoint:** `DELETE /api/superadmin/users/{userId}/roles`
- **Mô tả:** Xóa role khỏi user
- **Input:**
  ```json
  {
    "roleName": "Teacher"
  }
  ```

**Lưu ý:** Chức năng "Nâng cấp User thành Teacher" được FinanceAdmin quản lý qua `POST /api/admin/users/upgrade-to-teacher`, không phải SuperAdmin.

---

## 👁️ 3. XEM ROLES & PERMISSIONS (Read-only)

### 3.1. Xem danh sách Roles
- **Endpoint:** `GET /api/superadmin/roles`
- **Mô tả:** Xem danh sách tất cả roles trong hệ thống (fix cứng)
- **Response:** Danh sách roles kèm permissions và số lượng user

### 3.2. Xem danh sách Permissions
- **Endpoint:** `GET /api/superadmin/permissions`
- **Mô tả:** Xem danh sách tất cả permissions trong hệ thống (fix cứng)
- **Response:** Danh sách permissions với thông tin chi tiết

**Lưu ý:** SuperAdmin **KHÔNG THỂ** tạo/sửa/xóa roles và permissions (đã bị xóa để đơn giản hóa)

---

## 🔑 4. QUYỀN TRUY CẬP TẤT CẢ API CỦA ADMIN

SuperAdmin tự động có quyền truy cập **TẤT CẢ** các API của Admin, bao gồm:

### 4.1. Quản lý User (qua AdminManageUserController)
- `GET /api/admin/users` - Xem danh sách users
- `PUT /api/admin/users/block/{userId}` - Khóa tài khoản
- `PUT /api/admin/users/unblock/{userId}` - Mở khóa tài khoản
- `GET /api/admin/users/blocked` - Xem danh sách tài khoản bị khóa
- `GET /api/admin/users/teachers` - Xem danh sách teachers

### 4.2. Quản lý Course (qua ATCourseController)
- `GET /api/courses` - Xem danh sách courses
- `POST /api/courses/admin/create` - Tạo course (Admin)
- `PUT /api/courses/{courseId}` - Cập nhật course
- `DELETE /api/courses/{courseId}` - Xóa course
- Và tất cả các API khác của Admin

### 4.3. Quản lý Payment, Revenue, Package
- Tất cả các API liên quan đến tài chính

**Lý do:** `PermissionAuthorizationHandler` tự động cho phép SuperAdmin pass tất cả permission checks.

---

## 📊 TÓM TẮT QUYỀN

| Chức năng | Quyền | Ghi chú |
|-----------|-------|---------|
| **Tạo Admin** | ✅ Có | Tạo ContentAdmin hoặc FinanceAdmin |
| **Xem/Quản lý Admin** | ✅ Có | Xem, xóa, reset password, đổi email |
| **Gán/Xóa Role cho User** | ✅ Có | Quản lý roles của users |
| **Xem Roles & Permissions** | ✅ Có | Read-only, fix cứng |
| **Cập nhật Permissions riêng lẻ** | ❌ Không | Đã bị xóa (permissions fix cứng theo role) |
| **Tạo/Sửa/Xóa Roles** | ❌ Không | Đã bị xóa để đơn giản hóa |
| **Tạo/Sửa/Xóa Permissions** | ❌ Không | Đã bị xóa để đơn giản hóa |
| **Truy cập tất cả Admin APIs** | ✅ Có | Tự động pass permission checks |

---

## 🎯 Mục đích sử dụng SuperAdmin

1. **Quản lý hệ thống:** Tạo và quản lý các tài khoản admin
2. **Quản lý quyền:** Gán/xóa roles cho users
3. **Giám sát:** Xem danh sách roles và permissions (để hiểu cấu trúc hệ thống)
4. **Toàn quyền:** Truy cập tất cả các API của Admin để xử lý các tình huống đặc biệt

---

## ⚠️ Lưu ý

- **Roles và Permissions được fix cứng** trong seed data (DBContext)
- SuperAdmin **không thể** tạo/sửa/xóa roles và permissions
- Để thay đổi roles/permissions, cần sửa code trong `DBContext.cs` và `AdminPermissionSeeder.cs`, sau đó tạo migration mới


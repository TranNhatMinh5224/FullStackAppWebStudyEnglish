# SuperAdmin API List - Chỉ SuperAdmin mới được dùng

## 🔐 Authentication
Tất cả API yêu cầu JWT token với role `SuperAdmin` trong header:
```
Authorization: Bearer <token>
```

Base URL: `/api/superadmin`

---

## 📋 1. QUẢN LÝ ADMIN (Admin Management)

### 1.1. Tạo Admin mới
- **Method:** `POST`
- **Endpoint:** `/api/superadmin/admins`
- **Mô tả:** Tạo tài khoản admin mới (ContentAdmin hoặc FinanceAdmin)
- **Request Body:**
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
- **Response:** `201 Created` với thông tin admin vừa tạo

---

### 1.2. Lấy danh sách Admins
- **Method:** `GET`
- **Endpoint:** `/api/superadmin/admins`
- **Mô tả:** Lấy danh sách tất cả admins với phân trang
- **Query Parameters:**
  - `PageNumber` (int): Số trang (mặc định: 1)
  - `PageSize` (int): Số lượng items mỗi trang (mặc định: 10)
  - `SearchTerm` (string, optional): Tìm kiếm theo email, firstName, lastName
- **Response:** `200 OK` với danh sách admins

---

### 1.3. Xem chi tiết Admin
- **Method:** `GET`
- **Endpoint:** `/api/superadmin/admins/{userId}`
- **Mô tả:** Xem thông tin chi tiết của một admin
- **Path Parameters:**
  - `userId` (int): ID của admin
- **Response:** `200 OK` với thông tin admin

---

### 1.4. Xóa Admin
- **Method:** `DELETE`
- **Endpoint:** `/api/superadmin/admins/{userId}`
- **Mô tả:** Xóa admin (remove Admin role khỏi user)
- **Path Parameters:**
  - `userId` (int): ID của admin
- **Response:** `200 OK`

---

### 1.5. Reset Password Admin
- **Method:** `PUT`
- **Endpoint:** `/api/superadmin/admins/{userId}/reset-password`
- **Mô tả:** Reset password cho admin
- **Path Parameters:**
  - `userId` (int): ID của admin
- **Request Body:**
  ```json
  {
    "newPassword": "newpassword123"
  }
  ```
- **Response:** `200 OK`

---

### 1.6. Đổi Email Admin
- **Method:** `PUT`
- **Endpoint:** `/api/superadmin/admins/{userId}/email`
- **Mô tả:** Đổi email cho admin
- **Path Parameters:**
  - `userId` (int): ID của admin
- **Request Body:**
  ```json
  {
    "newEmail": "newemail@gmail.com"
  }
  ```
- **Response:** `200 OK`

---

## 👥 2. QUẢN LÝ USER ROLES (User Role Management)

### 2.1. Gán Role cho User
- **Method:** `POST`
- **Endpoint:** `/api/superadmin/users/{userId}/roles`
- **Mô tả:** Gán role cho user (SuperAdmin, ContentAdmin, FinanceAdmin, Teacher, Student)
- **Path Parameters:**
  - `userId` (int): ID của user
- **Request Body:**
  ```json
  {
    "roleName": "Teacher"
  }
  ```
- **Response:** `200 OK`

---

### 2.2. Xóa Role khỏi User
- **Method:** `DELETE`
- **Endpoint:** `/api/superadmin/users/{userId}/roles`
- **Mô tả:** Xóa role khỏi user
- **Path Parameters:**
  - `userId` (int): ID của user
- **Request Body:**
  ```json
  {
    "roleName": "Teacher"
  }
  ```
- **Response:** `200 OK`

---

## 👁️ 3. XEM ROLES & PERMISSIONS (Read-only)

### 3.1. Xem danh sách Roles
- **Method:** `GET`
- **Endpoint:** `/api/superadmin/roles`
- **Mô tả:** Xem danh sách tất cả roles trong hệ thống (fix cứng)
- **Response:** `200 OK` với danh sách roles kèm permissions và số lượng user
- **Response Example:**
  ```json
  {
    "success": true,
    "statusCode": 200,
    "data": [
      {
        "roleId": 1,
        "name": "SuperAdmin",
        "permissions": [...],
        "userCount": 1
      },
      {
        "roleId": 2,
        "name": "ContentAdmin",
        "permissions": [...],
        "userCount": 3
      }
    ]
  }
  ```

---

### 3.2. Xem danh sách Permissions
- **Method:** `GET`
- **Endpoint:** `/api/superadmin/permissions`
- **Mô tả:** Xem danh sách tất cả permissions trong hệ thống (fix cứng)
- **Response:** `200 OK` với danh sách permissions
- **Response Example:**
  ```json
  {
    "success": true,
    "statusCode": 200,
    "data": [
      {
        "permissionId": 1,
        "name": "Admin.Course.Manage",
        "displayName": "Quản lý khóa học",
        "description": "Tạo, sửa, xóa, publish khóa học",
        "category": "Content"
      }
    ]
  }
  ```

---

## 📊 TÓM TẮT API

| # | Method | Endpoint | Mô tả |
|---|--------|----------|-------|
| 1 | POST | `/api/superadmin/admins` | Tạo admin mới |
| 2 | GET | `/api/superadmin/admins` | Lấy danh sách admins |
| 3 | GET | `/api/superadmin/admins/{userId}` | Xem chi tiết admin |
| 4 | DELETE | `/api/superadmin/admins/{userId}` | Xóa admin |
| 5 | PUT | `/api/superadmin/admins/{userId}/reset-password` | Reset password admin |
| 6 | PUT | `/api/superadmin/admins/{userId}/email` | Đổi email admin |
| 7 | POST | `/api/superadmin/users/{userId}/roles` | Gán role cho user |
| 8 | DELETE | `/api/superadmin/users/{userId}/roles` | Xóa role khỏi user |
| 9 | GET | `/api/superadmin/roles` | Xem danh sách roles (read-only) |
| 10 | GET | `/api/superadmin/permissions` | Xem danh sách permissions (read-only) |

**Tổng cộng: 10 API endpoints**

---

## ⚠️ Lưu ý

1. **Permissions fix cứng:** Roles và Permissions được fix cứng trong seed data, không thể tạo/sửa/xóa qua API
2. **Toàn quyền:** SuperAdmin tự động pass tất cả permission checks, có thể truy cập tất cả API của Admin
3. **Nâng cấp User → Teacher:** Chức năng này được FinanceAdmin quản lý qua `/api/admin/users/upgrade-to-teacher`, không phải SuperAdmin


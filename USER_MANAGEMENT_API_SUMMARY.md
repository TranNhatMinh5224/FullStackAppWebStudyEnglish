# TỔNG HỢP API - USER MANAGEMENT & SUPERADMIN

## ✅ PERMISSION STATUS

### 1. ADMIN USER MANAGEMENT ENDPOINTS

**Controller:** `AdminManageUserController`  
**Route:** `/api/admin/users`  
**Base Authorization:** `[Authorize(Roles = "Admin")]`

| Endpoint | Method | Permission | Mô tả |
|----------|--------|------------|-------|
| `GET /api/admin/users` | GET | `[RequirePermission("Admin.User.Manage")]` | Lấy tất cả người dùng với phân trang |
| `PUT /api/admin/users/block/{userId}` | PUT | `[RequirePermission("Admin.User.Manage")]` | Khóa tài khoản người dùng |
| `PUT /api/admin/users/unblock/{userId}` | PUT | `[RequirePermission("Admin.User.Manage")]` | Mở khóa tài khoản người dùng |
| `GET /api/admin/users/blocked` | GET | `[RequirePermission("Admin.User.Manage")]` | Lấy danh sách tài khoản bị khóa |
| `GET /api/admin/users/teachers` | GET | `[RequirePermission("Admin.User.Manage")]` | Lấy danh sách giáo viên |

**Kết luận:** ✅ **ĐẦY ĐỦ** - Tất cả endpoints đều có `[RequirePermission("Admin.User.Manage")]`

---

### 2. SUPERADMIN ENDPOINTS

**Controller:** `SuperAdminController`  
**Route:** `/api/superadmin`  
**Base Authorization:** `[Authorize(Roles = "SuperAdmin")]`

**Lưu ý:** SuperAdmin có toàn quyền, không cần permission attributes (RLS sẽ bypass)

| Endpoint | Method | Mô tả |
|----------|--------|-------|
| `POST /api/superadmin/admins` | POST | Tạo admin mới |
| `GET /api/superadmin/admins` | GET | Lấy danh sách admins với phân trang |
| `GET /api/superadmin/admins/{userId}` | GET | Lấy chi tiết admin theo ID |
| `PUT /api/superadmin/admins/{userId}/permissions` | PUT | Cập nhật permissions của admin |
| `DELETE /api/superadmin/admins/{userId}` | DELETE | Xóa admin (remove Admin role) |
| `PUT /api/superadmin/admins/{userId}/reset-password` | PUT | Reset password admin |
| `PUT /api/superadmin/admins/{userId}/email` | PUT | Đổi email admin |
| `POST /api/superadmin/users/{userId}/roles` | POST | Gán role cho user |
| `DELETE /api/superadmin/users/{userId}/roles` | DELETE | Xóa role khỏi user |

**Kết luận:** ✅ **ĐẦY ĐỦ** - Tất cả endpoints chỉ dành cho SuperAdmin

---

### 3. ADMIN/TEACHER - COURSE STUDENT MANAGEMENT

**Controller:** `ATCourseController`  
**Route:** `/api/courses`  
**Base Authorization:** `[Authorize(Roles = "Admin, Teacher")]`

| Endpoint | Method | Permission | Mô tả |
|----------|--------|------------|-------|
| `GET /api/courses/{courseId}/students` | GET | `[Authorize(Roles = "Admin, Teacher")]` | Xem danh sách học viên trong khóa học |
| `GET /api/courses/{courseId}/students/{studentId}` | GET | `[Authorize(Roles = "Admin, Teacher")]` | Xem chi tiết học viên trong khóa học |
| `POST /api/courses/{courseId}/students` | POST | `[Authorize(Roles = "Admin, Teacher")]` | Thêm học viên vào khóa học |
| `DELETE /api/courses/{courseId}/students/{studentId}` | DELETE | `[Authorize(Roles = "Admin, Teacher")]` | Xóa học viên khỏi khóa học |

**Lưu ý:** 
- Admin có thể xem/thao tác trên tất cả courses (RLS)
- Teacher chỉ có thể xem/thao tác trên courses của mình (RLS)
- Không cần permission vì RLS đã filter theo ownership

---

## 📊 INPUT/OUTPUT DTOs

### ADMIN USER MANAGEMENT

#### 1. GET /api/admin/users

**Input (Query Parameters):**
```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "searchTerm": "email hoặc name" // Optional
}
```

**Output (ServiceResponse<PagedResult<UserDto>>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy danh sách users thành công",
  "data": {
    "items": [
      {
        "userId": 1,
        "email": "user@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "phoneNumber": "0123456789",
        "status": 1, // 1=Active, 2=Blocked
        "emailVerified": true,
        "createdAt": "2025-01-01T00:00:00Z",
        "roles": ["Student"]
      }
    ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 5
  }
}
```

---

#### 2. PUT /api/admin/users/block/{userId}

**Output (ServiceResponse<BlockAccountResponseDto>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Khóa tài khoản thành công",
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "status": 2, // Blocked
    "blockedAt": "2025-01-01T00:00:00Z"
  }
}
```

---

#### 3. PUT /api/admin/users/unblock/{userId}

**Output (ServiceResponse<UnblockAccountResponseDto>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Mở khóa tài khoản thành công",
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "status": 1, // Active
    "unblockedAt": "2025-01-01T00:00:00Z"
  }
}
```

---

### SUPERADMIN ENDPOINTS

#### 1. POST /api/superadmin/admins

**Input (CreateAdminDto):**
```json
{
  "email": "admin@example.com",
  "password": "SecurePassword123!",
  "firstName": "Admin",
  "lastName": "User",
  "phoneNumber": "0123456789",
  "permissionIds": [1, 2, 3, 4, 5, 6, 7] // Content: [1,2,3], Finance: [4,5,6,7]
}
```

**Output (ServiceResponse<AdminDto>):**
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Tạo admin thành công",
  "data": {
    "userId": 10,
    "email": "admin@example.com",
    "firstName": "Admin",
    "lastName": "User",
    "phoneNumber": "0123456789",
    "permissions": [
      {
        "permissionId": 1,
        "name": "Admin.Course.Manage",
        "displayName": "Quản lý khóa học"
      }
    ]
  }
}
```

---

#### 2. GET /api/superadmin/admins

**Input (Query Parameters):**
```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "searchTerm": "email hoặc name" // Optional
}
```

**Output (ServiceResponse<PagedResult<AdminDto>>):**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "items": [
      {
        "userId": 10,
        "email": "admin@example.com",
        "firstName": "Admin",
        "lastName": "User",
        "permissions": [...]
      }
    ],
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 1
  }
}
```

---

#### 3. PUT /api/superadmin/admins/{userId}/permissions

**Input (UpdateAdminPermissionsDto):**
```json
{
  "userId": 10, // Từ route, không cần trong body
  "permissionIds": [1, 2, 3] // Replace toàn bộ permissions
}
```

**Output (ServiceResponse<UpdateAdminPermissionsResultDto>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Cập nhật permissions thành công",
  "data": {
    "userId": 10,
    "removedPermissions": [
      { "permissionId": 4, "name": "Admin.User.Manage" }
    ],
    "addedPermissions": [
      { "permissionId": 1, "name": "Admin.Course.Manage" }
    ],
    "currentPermissions": [...]
  }
}
```

---

#### 4. POST /api/superadmin/users/{userId}/roles

**Input (AssignRoleDto):**
```json
{
  "userId": 1, // Từ route, không cần trong body
  "roleName": "Teacher"
}
```

**Output (ServiceResponse<RoleOperationResultDto>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Gán role 'Teacher' thành công",
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "roles": ["Student", "Teacher"]
  }
}
```

---

#### 5. DELETE /api/superadmin/users/{userId}/roles

**Input (RemoveRoleDto):**
```json
{
  "userId": 1, // Từ route, không cần trong body
  "roleName": "Teacher"
}
```

**Output (ServiceResponse<RoleOperationResultDto>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Xóa role 'Teacher' thành công",
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "roles": ["Student"]
  }
}
```

---

#### 6. PUT /api/superadmin/admins/{userId}/reset-password

**Input (ResetAdminPasswordDto):**
```json
{
  "userId": 10, // Từ route, không cần trong body
  "newPassword": "NewSecurePassword123!"
}
```

**Output (ServiceResponse<bool>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Reset password thành công",
  "data": true
}
```

---

#### 7. PUT /api/superadmin/admins/{userId}/email

**Input (ChangeAdminEmailDto):**
```json
{
  "userId": 10, // Từ route, không cần trong body
  "newEmail": "newadmin@example.com"
}
```

**Output (ServiceResponse<bool>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Đổi email thành công",
  "data": true
}
```

---

#### 8. DELETE /api/superadmin/admins/{userId}

**Output (ServiceResponse<RoleOperationResultDto>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Xóa admin thành công",
  "data": {
    "userId": 10,
    "email": "admin@example.com",
    "roles": ["Student"] // Admin role đã bị xóa
  }
}
```

---

### COURSE STUDENT MANAGEMENT

#### 1. GET /api/courses/{courseId}/students

**Input (Query Parameters):**
```json
{
  "pageNumber": 1,
  "pageSize": 20
}
```

**Output (ServiceResponse<PagedResult<UserDto>>):**
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "items": [
      {
        "userId": 1,
        "email": "student@example.com",
        "firstName": "Student",
        "lastName": "Name"
      }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 3
  }
}
```

---

#### 2. POST /api/courses/{courseId}/students

**Input (AddStudentToCourseDto):**
```json
{
  "email": "student@example.com"
}
```

**Output (ServiceResponse<bool>):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Thêm học viên vào khóa học thành công",
  "data": true
}
```

---

## 🔒 PERMISSION MAPPING

| Permission | Endpoints | Mục đích |
|------------|-----------|----------|
| `Admin.User.Manage` | `/api/admin/users/*` | Quản lý users (xem, block/unblock, xem teachers) |
| `SuperAdmin` (Role) | `/api/superadmin/*` | Toàn quyền quản lý Admin và Roles |
| `Admin, Teacher` (Role) | `/api/courses/{courseId}/students/*` | Quản lý students trong courses (RLS filter) |

---

## ✅ TỔNG KẾT

**Permission cho User Management:** ✅ **ĐẦY ĐỦ**

- Admin endpoints: Tất cả có `[RequirePermission("Admin.User.Manage")]`
- SuperAdmin endpoints: Chỉ `[Authorize(Roles = "SuperAdmin")]` (toàn quyền)
- Course Student Management: `[Authorize(Roles = "Admin, Teacher")]` + RLS filter
- RLS đã có sẵn cho Users table (Admin xem tất cả, Teacher chỉ xem students trong own courses)

---

## 📝 NOTES

- `AdminManageUserController` đã được cập nhật với permission đầy đủ
- `SuperAdminController` mới được tạo với đầy đủ chức năng quản lý Admin
- Tất cả endpoints đều có comments rõ ràng
- Routes đã được chuẩn hóa (ví dụ: `/block/{userId}` thay vì `/block-account/{userId}`)
- Response format đã được chuẩn hóa (ServiceResponse wrapper)


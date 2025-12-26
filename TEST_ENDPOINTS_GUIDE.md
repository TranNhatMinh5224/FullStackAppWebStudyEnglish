# HƯỚNG DẪN TEST: TẠO COURSE VÀ TẠO ADMIN

## 🚀 PROJECT ĐANG CHẠY

Project đã được khởi động ở background. Truy cập Swagger tại:
- **URL**: `http://localhost:5000/swagger` hoặc `https://localhost:5001/swagger`

---

## 📝 1. TẠO ADMIN (SuperAdmin)

### Endpoint:
```
POST /api/superadmin/admins
```

### Authorization:
- **Role**: `SuperAdmin`
- **Header**: `Authorization: Bearer <SuperAdmin_JWT_Token>`

### Request Body (CreateAdminDto):
```json
{
  "email": "admin@example.com",
  "password": "Admin123!@#",
  "firstName": "Admin",
  "lastName": "User",
  "phoneNumber": "0123456789",
  "permissionIds": [1, 2, 3, 4, 5, 6, 7, 8]
}
```

### Permission IDs (từ AdminPermissionSeeder):
- `1` - Admin.Course.Manage
- `2` - Admin.Lesson.Manage
- `3` - Admin.User.Manage
- `4` - Admin.Payment.Manage
- `5` - Admin.Package.Manage
- `6` - Admin.Content.Manage
- `7` - Admin.Finance.View
- `8` - Admin.Finance.Manage

**Ví dụ:**
- **Content Admin**: `[1, 2, 3, 6]` (Course, Lesson, User, Content)
- **Finance Admin**: `[4, 7, 8]` (Payment, Finance View, Finance Manage)
- **Full Admin**: `[1, 2, 3, 4, 5, 6, 7, 8]` (Tất cả permissions)

### Response (Success):
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Tạo admin thành công",
  "data": {
    "userId": 2,
    "email": "admin@example.com",
    "firstName": "Admin",
    "lastName": "User"
  }
}
```

---

## 📚 2. TẠO COURSE (Admin)

### Endpoint:
```
POST /api/courses/admin/create
```

### Authorization:
- **Role**: `Admin`
- **Permission**: `Admin.Course.Manage`
- **Header**: `Authorization: Bearer <Admin_JWT_Token>`

### Request Body (AdminCreateCourseRequestDto):
```json
{
  "title": "Khóa học tiếng Anh cơ bản",
  "description": "# Mô tả khóa học\n\nKhóa học dành cho người mới bắt đầu.",
  "imageTempKey": null,
  "imageType": null,
  "price": 500000,
  "maxStudent": 50,
  "isFeatured": false,
  "type": 1
}
```

### Fields:
- `title` (string, required): Tiêu đề khóa học
- `description` (string, required): Mô tả khóa học
- `imageTempKey` (string, optional): Key của image tạm trong MinIO (sẽ được move sang permanent location)
- `imageType` (string, optional): Loại image (jpg, png, etc.)
- `price` (decimal, optional): Giá khóa học (VND)
- `maxStudent` (int, required): Số lượng học viên tối đa
- `isFeatured` (bool, optional): Có highlight không (default: false)
- `type` (int, required): 
  - `1` = System Course (khóa học hệ thống)
  - `2` = Teacher Course (khóa học của giáo viên)

### Response (Success):
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Tạo khóa học thành công",
  "data": {
    "courseId": 1,
    "title": "Khóa học tiếng Anh cơ bản",
    "type": 1,
    "isPublished": true,
    "createdAt": "2025-12-26T10:00:00Z"
  }
}
```

---

## 🔐 3. ĐĂNG NHẬP ĐỂ LẤY JWT TOKEN

### Endpoint:
```
POST /api/auth/login
```

### Request Body:
```json
{
  "email": "minhxoandev@gmail.com",
  "password": "your_password"
}
```

### Response:
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "user": {
      "userId": 1,
      "email": "minhxoandev@gmail.com",
      "roles": ["SuperAdmin"]
    }
  }
}
```

**Copy `token` để dùng trong Authorization header!**

---

## 📋 TEST FLOW

### Bước 1: Đăng nhập SuperAdmin
1. POST `/api/auth/login` với email/password của SuperAdmin
2. Copy JWT token từ response

### Bước 2: Tạo Admin
1. POST `/api/superadmin/admins`
2. Header: `Authorization: Bearer <SuperAdmin_Token>`
3. Body: CreateAdminDto với permissions
4. Copy JWT token của Admin mới (hoặc login lại với email/password của Admin)

### Bước 3: Tạo Course
1. POST `/api/courses/admin/create`
2. Header: `Authorization: Bearer <Admin_Token>`
3. Body: AdminCreateCourseRequestDto
4. Kiểm tra response có `courseId`

---

## ✅ KIỂM TRA KẾT QUẢ

### Kiểm tra Admin đã được tạo:
```
GET /api/superadmin/admins
Authorization: Bearer <SuperAdmin_Token>
```

### Kiểm tra Course đã được tạo:
```
GET /api/courses/admin/all
Authorization: Bearer <Admin_Token>
```

---

## 🐛 TROUBLESHOOTING

### Lỗi 401 Unauthorized:
- Kiểm tra JWT token có đúng không
- Kiểm tra token chưa hết hạn
- Kiểm tra role trong token có đúng không

### Lỗi 403 Forbidden:
- Admin cần có permission `Admin.Course.Manage` để tạo course
- Kiểm tra `permissionIds` khi tạo Admin

### Lỗi 400 Bad Request:
- Kiểm tra request body có đúng format không
- Kiểm tra validation errors trong response

---

## 📝 NOTES

- **SuperAdmin** có toàn quyền, không cần permission
- **Admin** cần có permission `Admin.Course.Manage` để tạo course
- **RLS** sẽ tự động filter data theo role và permissions
- Tất cả endpoints đều có validation và error handling

Chúc bạn test thành công! 🎉


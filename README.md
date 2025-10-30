# 🎓 FULLSTACK APP WEB STUDY ENGLISH

## 📖 Giới thiệu

Ứng dụng học tiếng Anh toàn diện với:
- 👨‍💼 **Admin**: Quản lý khóa học, người dùng, giáo viên
- 👨‍🏫 **Teacher**: Tạo khóa học, quản lý học sinh, bài giảng
- 👨‍🎓 **Student**: Đăng ký khóa học, học bài, theo dõi tiến độ

---

## 🏗️ Kiến trúc

### Backend (ASP.NET Core 8.0)
```
Clean Architecture:
├── CleanDemo.API          → Controllers, Middleware
├── CleanDemo.Application  → Services, DTOs, Interfaces
├── CleanDemo.Domain       → Entities, Enums
└── CleanDemo.Infrastructure → DbContext, Repositories
```

### Frontend (React)
- React 18
- React Router
- Axios

---

## 🚀 Cài đặt & Chạy dự án

### ⚙️ Yêu cầu hệ thống
- .NET 8.0 SDK
- PostgreSQL 14+
- Node.js 18+
- Gmail Account (cho email service)

### 📥 Clone project
```bash
git clone https://github.com/TranNhatMinh5224/FullStackAppWebStudyEnglish.git
cd FullStackAppWebStudyEnglish
```

### 🔧 Cấu hình Backend

**Chi tiết xem:** [`BackendASP/CONFIGURATION.md`](BackendASP/CONFIGURATION.md)

```bash
# 1. Copy file cấu hình
cd BackendASP/CleanDemo.API
cp appsettings.json.example appsettings.Development.json

# 2. Sửa appsettings.Development.json với thông tin của bạn

# 3. Restore packages
cd ..
dotnet restore

# 4. Chạy migrations
dotnet ef database update --project CleanDemo.Infrastructure --startup-project CleanDemo.API

# 5. Chạy backend
cd CleanDemo.API
dotnet run
```

Backend sẽ chạy tại: `https://localhost:7074`

### 🎨 Cấu hình Frontend

```bash
cd Frontend
npm install
npm start
```

Frontend sẽ chạy tại: `http://localhost:3000`

---

## 🔑 Cấu hình quan trọng

### Database (PostgreSQL)
```json
{
  "Database": {
    "Server": "localhost",
    "Port": "5432",
    "Name": "Elearning",
    "User": "postgres",
    "Password": "your_password"
  }
}
```

### JWT Authentication
```json
{
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "FullStackAppWebStudyEnglish",
    "Audience": "FullStackAppWebStudyEnglish"
  }
}
```

### Email Service (Gmail)
```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "your-email@gmail.com",
    "Password": "your-gmail-app-password"
  }
}
```

**Tạo Gmail App Password:** https://myaccount.google.com/apppasswords

---

## 📚 API Documentation

**Swagger UI:** `https://localhost:7074/swagger`

### Authentication
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/forgot-password` - Quên mật khẩu

### Admin
- `GET /api/admin/courses` - Danh sách khóa học
- `POST /api/admin/courses` - Tạo khóa học
- `GET /api/admin/users` - Danh sách người dùng

### Teacher
- `GET /api/teacher/courses` - Khóa học của teacher
- `POST /api/teacher/courses` - Tạo khóa học
- `POST /api/teacher/lessons` - Tạo bài học

### User/Student
- `GET /api/courses` - Danh sách khóa học
- `POST /api/enrollments` - Đăng ký khóa học
- `GET /api/my-courses` - Khóa học đã đăng ký

---

## 🔐 Bảo mật

### Files KHÔNG được commit
❌ `appsettings.Development.json`
❌ `appsettings.Production.json`
❌ `.env` files

### Files an toàn để commit
✅ `appsettings.json` (template)
✅ `appsettings.json.example`

---

## 🛠️ Tech Stack

### Backend
- ASP.NET Core 8.0
- Entity Framework Core 8.0
- PostgreSQL
- JWT Authentication
- AutoMapper
- FluentValidation

### Frontend
- React 18
- React Router 6
- Axios

---

## 📁 Tài liệu chi tiết

- [`CONFIGURATION.md`](BackendASP/CONFIGURATION.md) - Hướng dẫn cấu hình
- [`MIGRATION_GUIDE.md`](BackendASP/MIGRATION_GUIDE.md) - Hướng dẫn migration từ .env
- [Swagger API](https://localhost:7074/swagger) - API Documentation

---

## 📧 Liên hệ

- GitHub: [@TranNhatMinh5224](https://github.com/TranNhatMinh5224)
- Project: [FullStackAppWebStudyEnglish](https://github.com/TranNhatMinh5224/FullStackAppWebStudyEnglish)

---

**Made with ❤️ by TranNhatMinh**

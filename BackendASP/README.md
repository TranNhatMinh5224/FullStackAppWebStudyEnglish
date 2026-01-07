# 📘 Catalunya English - Hệ Thống Quản Lý Học Tiếng Anh Trực Tuyến

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-336791?style=for-the-badge&logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7.0-DC382D?style=for-the-badge&logo=redis)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=for-the-badge&logo=docker)](https://www.docker.com/)

> **Catalunya English** là một hệ thống Backend mạnh mẽ, cung cấp giải pháp toàn diện cho việc học và giảng dạy tiếng Anh trực tuyến. Dự án được xây dựng dựa trên kiến trúc **Clean Architecture** hiện đại, đảm bảo khả năng mở rộng, bảo mật và hiệu năng cao.

---

## 📖 Giới thiệu Đề tài
Dự án tập trung vào việc giải quyết bài toán quản lý lộ trình học tập cá nhân hóa. Hệ thống không chỉ là một kho lưu trữ bài giảng mà còn tích hợp các công cụ hỗ trợ ghi nhớ (Flashcard), đánh giá năng lực (Quiz, Essay), và duy trì động lực học tập thông qua hệ thống tích điểm (Streak).

### Mục tiêu dự án:
- **Tối ưu trải nghiệm học tập**: Tích hợp từ vựng, ngữ pháp, phát âm trong một nền tảng duy nhất.
- **Hỗ trợ giảng viên**: Cung cấp công cụ quản lý khóa học và gói đăng ký (Subscription).
- **Quản trị thông minh**: Hệ thống báo cáo, thống kê và quản lý phân quyền (RBAC) chặt chẽ.

---

## ✨ Tính năng nổi bật

Hệ thống được phân chia chức năng theo 3 phân hệ chính:

### 👤 Dành cho Người học (User)
- **Lộ trình học tập**: Theo dõi tiến độ bài học (Lesson), chương học (Module) và khóa học (Course).
- **Luyện tập thông minh**: Hệ thống Flashcard hỗ trợ ghi nhớ, Quiz đa dạng loại câu hỏi.
- **Phát âm & Bài viết**: Nộp bài luận (Essay) và theo dõi tiến độ phát âm (Pronunciation).
- **Động lực học tập**: Hệ thống Streak hàng ngày và thông báo thời gian thực (SignalR/Email).
- **Thanh toán**: Đăng ký khóa học qua cổng thanh toán tích hợp.

### 👨‍🏫 Dành cho Giáo viên (Teacher)
- **Quản lý nội dung**: Soạn thảo bài giảng, tạo các bộ Quiz và Flashcard.
- **Gói dịch vụ**: Quản lý các gói Teacher Package và theo dõi doanh thu Subscription.
- **Tương tác**: Chấm bài luận và phản hồi kết quả cho học sinh.

### 🛡️ Dành cho Quản trị viên (Admin)
- **Dashboard**: Thống kê số lượng người dùng, doanh thu và lưu lượng truy cập.
- **Quản lý hệ thống**: Phê duyệt nội dung, quản lý quyền (Permissions) và vai trò (Roles).
- **Logging**: Theo dõi hoạt động hệ thống (Activity Log).

---

## 🏗️ Kiến trúc Hệ thống

Dự án áp dụng **Clean Architecture** kết hợp với **CQRS Pattern** (thông qua MediatR):

- **LearningEnglish.Domain**: Thực thể (Entities), Enum, Interface cơ bản. Không phụ thuộc vào bất kỳ layer nào khác.
- **LearningEnglish.Application**: Chứa Logic nghiệp vụ, DTOs, Validators (FluentValidation), và các Request/Response Handlers.
- **LearningEnglish.Infrastructure**: Cấu hình DB Context (EF Core), Migrations, Redis Cache, và các dịch vụ bên thứ 3 (Email, Payment).
- **LearningEnglish.API**: Các Controller RESTful, Middleware xử lý lỗi, Authentication (JWT/Google Auth).

---

## 🛠️ Công nghệ sử dụng

| Công nghệ | Mục đích |
| :--- | :--- |
| **ASP.NET Core 8** | Framework chính xây dựng Web API. |
| **Entity Framework Core** | ORM để giao tiếp với Cơ sở dữ liệu. |
| **PostgreSQL** | Hệ quản trị cơ sở dữ liệu quan hệ chính. |
| **Redis** | Lưu trữ Cache giúp tăng tốc độ phản hồi API. |
| **MediatR** | Triển khai mô hình CQRS và tách biệt logic xử lý. |
| **FluentValidation** | Kiểm tra tính hợp lệ của dữ liệu đầu vào. |
| **AutoMapper** | Ánh xạ tự động giữa Entities và DTOs. |
| **Docker** | Đóng gói ứng dụng và các dịch vụ đi kèm. |

---

## 🚀 Hướng dẫn Cài đặt chi tiết

### 📋 Yêu cầu hệ thống
- .NET 8.0 SDK
- Docker Desktop
- Postman (để test API)

### 1️⃣ Triển khai nhanh với Docker (Khuyên dùng)
Dự án đã được cấu hình sẵn Docker Compose bao gồm API, Database, Redis và Nginx.

```bash
# 1. Clone dự án
git clone https://github.com/TranNhatMinh5224/FullStackAppWebStudyEnglish.git
cd CatalunyaEnglish/FullStackAppWebStudyEnglish/BackendASP

# 2. Tạo file môi trường (Copy từ ví dụ)
cp .env.example .env.dev

# 3. Khởi chạy toàn bộ hệ thống
docker-compose -f docker-compose.dev.yml up -d --build
```

Hệ thống sẽ tự động khởi tạo:
- **API**: `http://localhost:5030`
- **Swagger**: `http://localhost:5030/swagger`
- **Postgres**: `localhost:5432`

### 2️⃣ Chạy trực tiếp trên máy local (Dành cho Dev)
1. **Cấu hình DB**: Cập nhật Connection String trong `appsettings.Development.json`.
2. **Migration**:
   ```bash
   dotnet ef database update --project LearningEnglish.Infrastructure --startup-project LearningEnglish.API
   ```
3. **Run**:
   ```bash
   dotnet run --project LearningEnglish.API
   ```

---

## 📂 Cấu trúc thư mục Source Code

```text
BackendASP/
├── LearningEnglish.API/           # Layer ngoại vi (Controller, Middleware)
├── LearningEnglish.Application/   # Business Logic (Services, DTOs, CQRS)
├── LearningEnglish.Domain/        # Core Logic (Entities, Interfaces)
├── LearningEnglish.Infrastructure/# Data Access, Migrations, External Services
└── LearningEnglish.Tests/         # Unit Tests & Integration Tests
```

---

## 🔐 Bảo mật & Quy chuẩn
- **Authentication**: Sử dụng JWT (JSON Web Token) kết hợp Refresh Token.
- **Authorization**: Phân quyền dựa trên Role và Permission (RBAC).
- **Validation**: Mọi dữ liệu đầu vào đều được validate chặt chẽ ở Application Layer.
- **Error Handling**: Middleware tập trung xử lý lỗi và trả về định dạng chuẩn.

---

## 📬 Liên hệ
Nếu bạn có bất kỳ câu hỏi nào về dự án, vui lòng liên hệ:
- **Nhóm thực hiện**: Nhóm 8
- **Email**: minhxoandev@gmail.com
- **Dự án**: FullStack English Learning Platform

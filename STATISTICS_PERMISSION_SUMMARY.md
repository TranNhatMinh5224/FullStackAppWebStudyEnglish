# TỔNG HỢP PERMISSION - STATISTICS/DASHBOARD

## ✅ PERMISSION STATUS - ADMIN STATISTICS ENDPOINTS

**Controller:** `AdminStatisticsController`  
**Route:** `/api/admin/statistics`  
**Base Authorization:** `[Authorize(Roles = "Admin")]`

### Endpoints với Permission:

| Endpoint | Method | Permission | Mô tả |
|----------|--------|------------|-------|
| `GET /api/admin/statistics/overview` | GET | `[RequirePermission("Admin.Revenue.View")]` | Tổng quan dashboard (users, courses, revenue) |
| `GET /api/admin/statistics/revenue` | GET | `[RequirePermission("Admin.Revenue.View")]` | Chi tiết thống kê doanh thu |
| `GET /api/admin/statistics/revenue/chart` | GET | `[RequirePermission("Admin.Revenue.View")]` | Dữ liệu doanh thu cho biểu đồ |
| `GET /api/admin/statistics/users` | GET | `[RequirePermission("Admin.User.Manage")]` | Thống kê users |
| `GET /api/admin/statistics/courses` | GET | `[RequirePermission("Admin.Course.Manage")]` | Thống kê courses |
| `GET /api/admin/statistics/teachers` | GET | `[RequirePermission("Admin.User.Manage")]` | Thống kê teachers |
| `GET /api/admin/statistics/students` | GET | `[RequirePermission("Admin.User.Manage")]` | Thống kê students |

---

## 📊 INPUT/OUTPUT DTOs

### 1. GET /api/admin/statistics/overview

#### Output (ServiceResponse<AdminOverviewStatisticsDto>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy thống kê tổng quan thành công",
  "data": {
    "totalUsers": 1000,
    "totalStudents": 800,
    "totalTeachers": 150,
    "totalAdmins": 50,
    "totalCourses": 200,
    "totalSystemCourses": 100,
    "totalTeacherCourses": 100,
    "totalEnrollments": 5000,
    "totalRevenue": 50000000,
    "newUsersLast30Days": 50,
    "newCoursesLast30Days": 10
  }
}
```

---

### 2. GET /api/admin/statistics/revenue

#### Output (ServiceResponse<RevenueStatisticsDto>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy thống kê doanh thu thành công",
  "data": {
    "totalRevenue": 50000000,
    "completedRevenue": 48000000,
    "pendingRevenue": 2000000,
    "revenueToday": 500000,
    "revenueThisWeek": 5000000,
    "revenueThisMonth": 20000000,
    "revenueThisYear": 50000000,
    "totalTransactions": 1000,
    "completedTransactions": 960,
    "pendingTransactions": 30,
    "failedTransactions": 10,
    "averageTransactionValue": 50000,
    "transactionsToday": 10,
    "transactionsThisWeek": 100,
    "transactionsThisMonth": 400
  }
}
```

---

### 3. GET /api/admin/statistics/revenue/chart

#### Input (Query Parameters):
```
?days=30  // Optional, default: 30
```

#### Output (ServiceResponse<RevenueChartDto>):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy dữ liệu biểu đồ doanh thu thành công",
  "data": {
    "totalRevenue": 50000000,
    "courseRevenue": 30000000,
    "teacherPackageRevenue": 20000000,
    "dailyRevenue": [
      { "date": "2025-12-01T00:00:00Z", "amount": 500000 },
      { "date": "2025-12-02T00:00:00Z", "amount": 600000 }
    ],
    "monthlyRevenue": [
      { "date": "2025-01-01T00:00:00Z", "amount": 5000000 },
      { "date": "2025-02-01T00:00:00Z", "amount": 6000000 }
    ],
    "dailyCourseRevenue": [...],
    "dailyTeacherPackageRevenue": [...]
  }
}
```

---

### 4. GET /api/admin/statistics/users

#### Output (ServiceResponse<UserStatisticsDto>):
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "totalUsers": 1000,
    "totalStudents": 800,
    "totalTeachers": 150,
    "totalAdmins": 50,
    "activeUsers": 950,
    "blockedUsers": 50,
    "newUsersToday": 5,
    "newUsersThisWeek": 30,
    "newUsersThisMonth": 100
  }
}
```

---

### 5. GET /api/admin/statistics/courses

#### Output (ServiceResponse<CourseStatisticsDto>):
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "totalCourses": 200,
    "systemCourses": 100,
    "teacherCourses": 100,
    "publishedCourses": 180,
    "draftCourses": 20,
    "newCoursesThisMonth": 10,
    "totalEnrollments": 5000,
    "averageEnrollmentsPerCourse": 25
  }
}
```

---

### 6. GET /api/admin/statistics/teachers

#### Output (ServiceResponse<TeacherStatisticsDto>):
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "totalTeachers": 150,
    "activeTeachers": 140,
    "blockedTeachers": 10,
    "newTeachersToday": 1,
    "newTeachersThisWeek": 5,
    "newTeachersThisMonth": 20,
    "totalCoursesCreated": 100,
    "publishedCoursesCreated": 90,
    "averageCoursesPerTeacher": 0.67,
    "totalEnrollmentsForTeacherCourses": 2000
  }
}
```

---

### 7. GET /api/admin/statistics/students

#### Output (ServiceResponse<StudentStatisticsDto>):
```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "totalStudents": 800,
    "activeStudents": 750,
    "blockedStudents": 50,
    "newStudentsToday": 4,
    "newStudentsThisWeek": 25,
    "newStudentsThisMonth": 80,
    "totalEnrollments": 5000,
    "studentsWithEnrollments": 600,
    "averageEnrollmentsPerStudent": 6.25,
    "activeStudentsInCourses": 600
  }
}
```

---

## 🔒 PERMISSION MAPPING

| Permission | Endpoints | Mục đích |
|------------|-----------|----------|
| `Admin.Revenue.View` | `/overview`, `/revenue`, `/revenue/chart` | Xem thống kê doanh thu |
| `Admin.User.Manage` | `/users`, `/teachers`, `/students` | Xem thống kê users |
| `Admin.Course.Manage` | `/courses` | Xem thống kê courses |

---

## ✅ TỔNG KẾT

**Permission cho Statistics/Dashboard:** ✅ **ĐẦY ĐỦ**

- Tất cả endpoints đều có permission phù hợp
- Revenue statistics: `Admin.Revenue.View`
- User statistics: `Admin.User.Manage`
- Course statistics: `Admin.Course.Manage`
- RLS đã có sẵn cho Payments table (Admin có thể xem tất cả với permission)

---

## 📝 NOTES

- `AdminStatisticsService` đã được register trong `Program.cs`
- `AdminStatisticsController` mới được tạo với đầy đủ permission
- Revenue statistics sử dụng `PaymentRepository` methods (đã có RLS)
- Tất cả endpoints đều có comments rõ ràng


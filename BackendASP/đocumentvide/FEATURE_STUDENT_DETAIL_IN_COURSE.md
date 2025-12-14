# ✅ CHỨC NĂNG XEM THÔNG TIN CHI TIẾT HỌC SINH TRONG COURSE

## 📅 Ngày: 14/12/2025

---

## 🎯 YÊU CẦU

Xem thông tin chi tiết của học sinh trong một khóa học cụ thể, bao gồm:
- ✅ Thông tin cơ bản: Họ, tên, giới tính, ngày sinh, avatar
- ✅ Ngày tham gia course
- ✅ Tiến độ hoàn thành course (CompletedLessons/TotalLessons, ProgressPercentage)

### 🔒 Phân quyền:
- **Admin**: Xem tất cả students trong tất cả courses
- **Teacher**: Chỉ xem students trong own courses (RLS tự động filter)

---

## 📝 FILES ĐÃ TẠO/SỬA (7 FILES)

### 1️⃣ **StudentDetailDto.cs** (NEW)
```
BackendASP/LearningEnglish.Application/DTOS/StudentDetailDto.cs
```

**DTOs mới:**
- `StudentDetailInCourseDto` - Thông tin chi tiết học sinh trong course
- `CourseProgressDetailDto` - Chi tiết tiến độ học tập

**Properties:**
```csharp
StudentDetailInCourseDto:
- UserId, FirstName, LastName, DisplayName
- Email, DateOfBirth, IsMale, AvatarUrl
- CourseId, CourseName
- JoinedAt (DateTime)
- Progress (CourseProgressDetailDto)

CourseProgressDetailDto:
- CompletedLessons, TotalLessons
- ProgressPercentage, IsCompleted
- CompletedAt, LastUpdated
- ProgressDisplay (string: "5/10 (50.0%)")
```

---

### 2️⃣ **UserService.cs** (UPDATED)

#### Constructor - Thêm dependency:
```csharp
+ private readonly ICourseProgressRepository _courseProgressRepository;

+ ICourseProgressRepository courseProgressRepository // trong constructor
```

#### Method mới:
```csharp
public async Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(
    int courseId, 
    int studentId, 
    int currentUserId, 
    string currentUserRole)
```

**Logic:**
1. ✅ RLS tự động filter course theo role
2. ✅ Kiểm tra student existence
3. ✅ Kiểm tra student có enrolled trong course không
4. ✅ Lấy thông tin CourseProgress
5. ✅ Build avatar URL từ BuildPublicUrl helper
6. ✅ Map sang StudentDetailInCourseDto
7. ✅ Handle trường hợp chưa có progress record (default values)

---

### 3️⃣ **IUserManagementService.cs** (UPDATED)

Thêm method signature:
```csharp
Task<ServiceResponse<StudentDetailInCourseDto>> GetStudentDetailInCourseAsync(
    int courseId, 
    int studentId, 
    int currentUserId, 
    string currentUserRole);
```

---

### 4️⃣ **ICourseRepository.cs** (UPDATED)

Thêm method signature:
```csharp
Task<UserCourse?> GetUserCourseAsync(int userId, int courseId);
```

**Mục đích:** Lấy thông tin enrollment (JoinedAt, PaymentId) của student trong course

---

### 5️⃣ **CourseRepository.cs** (UPDATED)

Implement method:
```csharp
public async Task<UserCourse?> GetUserCourseAsync(int userId, int courseId)
{
    return await _context.UserCourses
        .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);
}
```

---

### 6️⃣ **ATCourseController.cs** (UPDATED)

Thêm endpoint mới:
```csharp
// GET: api/courses/{courseId}/students/{studentId}
[HttpGet("{courseId}/students/{studentId}")]
[Authorize(Roles = "Admin, Teacher")]
public async Task<IActionResult> GetStudentDetailInCourse(int courseId, int studentId)
```

**Flow:**
1. Extract userId & role từ JWT
2. Gọi `_userManagementService.GetStudentDetailInCourseAsync()`
3. Return response

---

## 🔒 RLS SECURITY

### ✅ RLS Policies đã có sẵn:

**Courses Table:**
- `courses_policy_admin_select` - Admin thấy tất cả courses
- `courses_policy_teacher_select_own` - Teacher chỉ thấy own courses

**UserCourses Table:**
- `usercourses_policy_admin_select` - Admin thấy tất cả enrollments
- `usercourses_policy_teacher_select_own_courses` - Teacher chỉ thấy students trong own courses

### 🔐 Security Flow:

```
Teacher calls: GET /api/courses/123/students/456

1. RLS Middleware sets:
   - app.current_user_id = teacherId
   - app.current_user_role = 'Teacher'

2. Service checks course (line: var course = await _courseRepository.GetByIdAsync(courseId)):
   - RLS policy: courses_policy_teacher_select_own
   - PostgreSQL auto filters: WHERE TeacherId = current_user_id
   - Result: course == null if not owned by teacher

3. Service checks enrollment (line: var userCourse = await _courseRepository.GetUserCourseAsync(...)):
   - RLS policy: usercourses_policy_teacher_select_own_courses
   - PostgreSQL auto filters via EXISTS(Courses.TeacherId = current_user_id)
   - Result: userCourse == null if student not in teacher's course

4. If both checks pass → Return student detail
   If any fails → Return 404 (security best practice)
```

---

## 📊 RESPONSE EXAMPLE

### ✅ Success Response (200):
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Lấy thông tin học sinh thành công",
  "data": {
    "userId": 456,
    "firstName": "Nguyễn",
    "lastName": "Văn A",
    "displayName": "Nguyễn Văn A",
    "email": "nguyenvana@example.com",
    "dateOfBirth": "2000-01-15",
    "isMale": true,
    "avatarUrl": "https://minio.example.com/avatars/real/abc123.jpg",
    "courseId": 123,
    "courseName": "English for Beginners",
    "joinedAt": "2024-12-01T10:30:00Z",
    "progress": {
      "completedLessons": 5,
      "totalLessons": 10,
      "progressPercentage": 50.0,
      "isCompleted": false,
      "completedAt": null,
      "lastUpdated": "2024-12-14T08:20:00Z",
      "progressDisplay": "5/10 (50.0%)"
    }
  }
}
```

### ❌ Error Response (404 - Student not in teacher's course):
```json
{
  "success": false,
  "statusCode": 404,
  "message": "Không tìm thấy khóa học hoặc bạn không có quyền truy cập",
  "data": null
}
```

### ❌ Error Response (404 - Student not enrolled):
```json
{
  "success": false,
  "statusCode": 404,
  "message": "Học sinh chưa tham gia khóa học này",
  "data": null
}
```

---

## 🧪 TESTING SCENARIOS

### **1. Teacher xem student trong own course:**
```bash
GET /api/courses/123/students/456
Authorization: Bearer <teacher_token>

Expected: 200 OK với student detail
```

### **2. Teacher xem student trong course của teacher khác:**
```bash
GET /api/courses/999/students/456
Authorization: Bearer <teacher_token>

Expected: 404 "Không tìm thấy khóa học hoặc bạn không có quyền truy cập"
```

### **3. Admin xem bất kỳ student nào:**
```bash
GET /api/courses/123/students/456
Authorization: Bearer <admin_token>

Expected: 200 OK với student detail
```

### **4. Teacher xem student chưa enroll:**
```bash
GET /api/courses/123/students/789
Authorization: Bearer <teacher_token>

Expected: 404 "Học sinh chưa tham gia khóa học này"
```

### **5. Student chưa có progress record:**
```bash
# Student mới enroll, chưa học lesson nào

Expected: 200 OK với progress = {
  completedLessons: 0,
  totalLessons: 10,
  progressPercentage: 0,
  progressDisplay: "0/10 (0.0%)"
}
```

---

## 🎯 FEATURES IMPLEMENTED

| Feature | Status | Note |
|---------|--------|------|
| **Thông tin cơ bản học sinh** | ✅ | FirstName, LastName, Email, Gender, DOB |
| **Avatar URL** | ✅ | Dùng BuildPublicUrl helper có sẵn |
| **Ngày tham gia course** | ✅ | Từ UserCourse.JoinedAt |
| **Tiến độ hoàn thành** | ✅ | Từ CourseProgress entity |
| **Handle no progress** | ✅ | Default values nếu chưa có record |
| **RLS Security** | ✅ | Auto filter theo Teacher's courses |
| **Admin full access** | ✅ | Admin thấy tất cả students |

---

## 📚 DEPENDENCIES SỬ DỤNG

### **Entities có sẵn:**
- ✅ `User` - Thông tin học sinh
- ✅ `UserCourse` - Enrollment info (JoinedAt)
- ✅ `CourseProgress` - Tiến độ học tập
- ✅ `Course` - Thông tin khóa học

### **Repositories có sẵn:**
- ✅ `IUserRepository.GetByIdAsync()` - Lấy user info
- ✅ `ICourseRepository.GetByIdAsync()` - Kiểm tra course (RLS filter)
- ✅ `ICourseRepository.GetUserCourseAsync()` - **MỚI THÊM** - Lấy enrollment
- ✅ `ICourseProgressRepository.GetByUserAndCourseAsync()` - Lấy progress

### **Helpers có sẵn:**
- ✅ `BuildPublicUrl.BuildURL()` - Build avatar URL từ MinIO key

---

## 🚀 NEXT STEPS (Optional Extensions)

### **1. Thêm thông tin Quiz Attempts:**
- Số lần làm quiz
- Điểm trung bình
- Best score

### **2. Thêm thông tin Essay Submissions:**
- Số bài essay đã nộp
- Số bài đã được chấm điểm

### **3. Thêm thông tin Streak:**
- Current streak
- Longest streak
- Last activity date

### **4. Export PDF:**
- Tạo endpoint export student progress report PDF
- Dùng cho giáo viên in báo cáo

---

## ✅ COMPLETION STATUS

| Component | Status |
|-----------|--------|
| **DTO** | ✅ Created |
| **Repository** | ✅ Method added |
| **Service** | ✅ Method implemented |
| **Controller** | ✅ Endpoint created |
| **RLS Security** | ✅ Works automatically |
| **Build** | ✅ Success (no errors) |
| **Testing** | ⏳ Pending manual test |

---

**Created by:** GitHub Copilot  
**Date:** December 14, 2025  
**Build Status:** ✅ Build succeeded  
**Ready for Testing:** ✅ Yes

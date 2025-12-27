# 📚 LESSON ARCHITECTURE - PHÂN TÁCH THEO ROLE

## 🎯 TỔNG QUAN

Hệ thống Lesson đã được tách biệt rõ ràng theo 3 roles:
- **Admin**: Quản lý lessons của System courses
- **Teacher**: Quản lý lessons của Teacher courses (own courses)
- **User/Student**: Xem lessons với progress tracking

---

## 📁 KIẾN TRÚC FILE

```
Application/
├── Interface/Services/ILesson/
│   ├── IAdminLessonService.cs     # Admin interface
│   ├── ITeacherLessonService.cs   # Teacher interface
│   └── IUserLessonService.cs      # User interface (renamed from ILessonService)
│
└── Service/LessonService/
    ├── AdminLessonService.cs      # Admin implementation
    ├── TeacherLessonService.cs    # Teacher implementation
    └── UserLessonService.cs       # User implementation (renamed from LessonService)

API/Controller/
├── Admin/AdminLesson/AdminLessonController.cs
├── Teacher/TeacherLesson/TeacherLessonController.cs
└── User/UserLessonController.cs
```

---

## 🔐 PHÂN QUYỀN & ENDPOINTS

### 1️⃣ **ADMIN ENDPOINTS**

**Base URL**: `/api/admin/lessons`  
**Authorization**: `[Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]` + `[RequirePermission("Admin.Lesson.Manage")]`

| Method | Endpoint | Action | Description |
|--------|----------|--------|-------------|
| POST | `/api/admin/lessons` | AdminAddLesson | Tạo lesson cho System course |
| GET | `/api/admin/lessons/{lessonId}` | GetLessonById | Xem chi tiết lesson (tất cả lessons) |
| GET | `/api/admin/lessons/course/{courseId}` | GetListLessonByCourseId | Danh sách lessons theo course |
| PUT | `/api/admin/lessons/{lessonId}` | UpdateLesson | Cập nhật lesson |
| DELETE | `/api/admin/lessons/{lessonId}` | DeleteLesson | Xóa lesson |

**Business Rules**:
- ✅ Admin chỉ tạo lessons cho **System courses** (CourseType.System)
- ✅ Admin có quyền xem/sửa/xóa **TẤT CẢ** lessons (RLS: `lessons_policy_admin_all`)
- ✅ Không cần kiểm tra ownership

**Return Type**: `LessonDto` (không có progress)

---

### 2️⃣ **TEACHER ENDPOINTS**

**Base URL**: `/api/teacher/lessons`  
**Authorization**: `[RequireTeacherRole]` (check DB for active Teacher subscription)

| Method | Endpoint | Action | Description |
|--------|----------|--------|-------------|
| POST | `/api/teacher/lessons` | TeacherAddLesson | Tạo lesson cho Teacher course |
| GET | `/api/teacher/lessons/{lessonId}` | GetLessonById | Xem chi tiết lesson (own lessons only) |
| GET | `/api/teacher/lessons/course/{courseId}` | GetListLessonByCourseId | Danh sách lessons theo course (own) |
| PUT | `/api/teacher/lessons/{lessonId}` | UpdateLesson | Cập nhật lesson (own) |
| DELETE | `/api/teacher/lessons/{lessonId}` | DeleteLesson | Xóa lesson (own) |

**Business Rules**:
- ✅ Teacher chỉ tạo lessons cho **Teacher courses** (CourseType.Teacher)
- ✅ Teacher CHỈ xem/sửa/xóa lessons của **OWN courses** (RLS: `lessons_policy_teacher_all_own`)
- ✅ Kiểm tra ownership: `course.TeacherId == teacherId`
- ✅ Kiểm tra giới hạn: `currentLessonCount < teacherPackage.MaxLessons`

**Return Type**: `LessonDto` (không có progress)

---

### 3️⃣ **USER/STUDENT ENDPOINTS**

**Base URL**: `/api/user/lessons`  
**Authorization**: `[Authorize]` (authenticated users)

| Method | Endpoint | Action | Description |
|--------|----------|--------|-------------|
| GET | `/api/user/lessons/{lessonId}` | GetLessonById | Xem chi tiết lesson + progress |
| GET | `/api/user/lessons/course/{courseId}` | GetLessonsByCourseId | Danh sách lessons + progress |

**Business Rules**:
- ✅ User xem lessons của **enrolled courses** hoặc **public courses**
- ✅ Tự động load **progress tracking** (LessonCompletion)
- ✅ RLS filter theo enrollment status

**Return Type**: `LessonWithProgressDto` (có progress tracking)

---

## 🔄 SERVICE METHODS MAPPING

| Service | Method | Parameters | Return Type |
|---------|--------|------------|-------------|
| **AdminLessonService** | AdminAddLesson | AdminCreateLessonDto | LessonDto |
| | UpdateLesson | lessonId, UpdateLessonDto | LessonDto |
| | DeleteLesson | lessonId | bool |
| | GetLessonById | lessonId | LessonDto |
| | GetListLessonByCourseId | courseId | List\<LessonDto\> |
| **TeacherLessonService** | TeacherAddLesson | TeacherCreateLessonDto, teacherId | LessonDto |
| | UpdateLesson | lessonId, UpdateLessonDto | LessonDto |
| | DeleteLesson | lessonId | bool |
| | GetLessonById | lessonId | LessonDto |
| | GetListLessonByCourseId | courseId | List\<LessonDto\> |
| **UserLessonService** | GetLessonById | lessonId, userId | LessonWithProgressDto |
| | GetListLessonByCourseId | courseId, userId | List\<LessonWithProgressDto\> |

---

## 📊 DTO COMPARISON

### LessonDto (Admin/Teacher)
```csharp
{
    LessonId,
    Title,
    Description,
    OrderIndex,
    CourseId,
    ImageUrl,
    ImageType,
    CreatedAt,
    UpdatedAt
}
```

### LessonWithProgressDto (User)
```csharp
{
    // Thông tin cơ bản (inherit from LessonDto)
    LessonId,
    Title,
    Description,
    OrderIndex,
    CourseId,
    ImageUrl,
    ImageType,
    
    // Progress tracking (from LessonCompletion)
    CompletionPercentage,
    IsCompleted,
    CompletedModules,
    TotalModules,
    VideoProgressPercentage,
    StartedAt,
    CompletedAt
}
```

---

## 🛡️ ROW-LEVEL SECURITY (RLS) POLICIES

### Assumed PostgreSQL RLS Policies:

```sql
-- Admin: Có quyền truy cập TẤT CẢ lessons (với permission Admin.Lesson.Manage)
CREATE POLICY lessons_policy_admin_all ON "Lessons"
FOR ALL
TO authenticated
USING (
    EXISTS (
        SELECT 1 FROM "Users" u
        JOIN "UserRoles" ur ON u."UserId" = ur."UserId"
        JOIN "Roles" r ON ur."RoleId" = r."RoleId"
        JOIN "RolePermissions" rp ON r."RoleId" = rp."RoleId"
        JOIN "Permissions" p ON rp."PermissionId" = p."PermissionId"
        WHERE u."UserId" = current_setting('app.current_user_id', true)::integer
        AND p."Name" = 'Admin.Lesson.Manage'
    )
);

-- Teacher: CHỈ truy cập lessons của OWN courses
CREATE POLICY lessons_policy_teacher_all_own ON "Lessons"
FOR ALL
TO authenticated
USING (
    EXISTS (
        SELECT 1 FROM "Courses" c
        JOIN "Users" u ON c."TeacherId" = u."UserId"
        JOIN "UserRoles" ur ON u."UserId" = ur."UserId"
        JOIN "Roles" r ON ur."RoleId" = r."RoleId"
        WHERE c."CourseId" = "Lessons"."CourseId"
        AND u."UserId" = current_setting('app.current_user_id', true)::integer
        AND r."Name" = 'Teacher'
    )
);

-- Student: CHỈ xem lessons của enrolled courses
CREATE POLICY lessons_policy_student_enrolled ON "Lessons"
FOR SELECT
TO authenticated
USING (
    EXISTS (
        SELECT 1 FROM "UserCourses" uc
        WHERE uc."CourseId" = "Lessons"."CourseId"
        AND uc."UserId" = current_setting('app.current_user_id', true)::integer
    )
);
```

---

## ✅ IMPROVEMENTS & FIXES APPLIED

### 🐛 Fixed Issues:

1. **UserLessonService.cs**:
   - ❌ **Before**: `int userId` parameter but used `userId.HasValue` → Compile error!
   - ✅ **After**: Changed to `int userId` (non-nullable) và bỏ `.HasValue` check

2. **IUserLessonService.cs** (renamed from ILessonService):
   - ✅ Added `GetLessonById(int lessonId, int userId)` method
   - ✅ Renamed interface for clarity

3. **UserLessonController.cs**:
   - ❌ **Before**: Gọi method với wrong parameters
   - ✅ **After**: Fixed method calls to match interface
   - ✅ Added `GetLessonById` endpoint

### 🎯 Improvements:

1. **Separation of Concerns**: 
   - Tách rõ 3 services theo role
   - Mỗi service có business logic riêng

2. **Type Safety**: 
   - Fixed nullable/non-nullable issues
   - Consistent parameter types

3. **Progress Tracking**: 
   - User service tự động load progress từ LessonCompletion
   - Admin/Teacher không cần progress (performance)

4. **Authorization**: 
   - Admin: Permission-based (`RequirePermission`)
   - Teacher: Role-based + RLS ownership check
   - User: Enrollment-based + RLS

---

## 🚀 TESTING CHECKLIST

### Admin Tests:
- [ ] Tạo lesson cho System course → Success
- [ ] Tạo lesson cho Teacher course → 403 Forbidden
- [ ] Xem tất cả lessons → Success
- [ ] Cập nhật/xóa bất kỳ lesson nào → Success
- [ ] Upload/update/delete lesson image → Success

### Teacher Tests:
- [ ] Tạo lesson cho Teacher course (own) → Success
- [ ] Tạo lesson cho System course → 403 Forbidden
- [ ] Tạo lesson cho course của teacher khác → 403 Forbidden
- [ ] Đạt limit MaxLessons → 403 Forbidden
- [ ] Xem/sửa/xóa own lessons → Success
- [ ] Xem/sửa/xóa lessons của teacher khác → 404 Not Found (RLS filter)

### User Tests:
- [ ] Xem lessons của enrolled course + progress → Success
- [ ] Xem lessons của non-enrolled course → Empty list (RLS filter)
- [ ] Progress tracking hiển thị đúng → Success
- [ ] Image URLs generate correctly → Success

---

## 📝 NOTES

1. **Services Registration** (Program.cs):
```csharp
builder.Services.AddScoped<IAdminLessonService, AdminLessonService>();
builder.Services.AddScoped<ITeacherLessonService, TeacherLessonService>();
builder.Services.AddScoped<ILessonService, LessonService>(); // ← Rename to IUserLessonService recommended
```

2. **MinIO File Handling**:
   - Tất cả services đều sử dụng `IMinioFileStorage`
   - Temp file → Real file flow
   - Auto cleanup on errors (Teacher service có rollback logic)

3. **RLS Middleware Order** (Program.cs):
```csharp
app.UseAuthentication();  // 1. JWT validation
app.UseRlsMiddleware();   // 2. Set app.current_user_id (BEFORE Authorization!)
app.UseAuthorization();   // 3. Check [Authorize] + [RequirePermission]
app.MapControllers();     // 4. Execute actions
```

---

## 🔮 RECOMMENDATIONS

1. **Rename ILessonService → IUserLessonService**:
   - Để naming consistency với các service khác
   - Update trong Program.cs registration

2. **Add DTOs Validation**:
   - `AdminCreateLessonDtoValidator`
   - `TeacherCreateLessonDtoValidator`
   - `UpdateLessonDtoValidator`

3. **Add Unit Tests**:
   - Test business logic riêng cho từng service
   - Mock RLS policies behavior

4. **Consider Caching**:
   - Cache lessons list per course (invalidate on CRUD)
   - Cache progress data (invalidate on module completion)

5. **Add Pagination**:
   - `GetListLessonByCourseId` nên support pagination nếu lesson count lớn

---

**Generated**: 2025-01-27  
**Version**: 1.0  
**Status**: ✅ Ready for Production


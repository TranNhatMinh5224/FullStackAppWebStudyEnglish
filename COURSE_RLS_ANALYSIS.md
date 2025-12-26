# PHÂN TÍCH RLS POLICIES CHO COURSES

## 📋 TỔNG QUAN

### Endpoints liên quan đến Course:

#### 1. **Guest Endpoints** (AllowAnonymous)
- `GET /api/user/courses/system-courses` - Xem danh sách system courses
- `GET /api/user/courses/{courseId}` - Xem chi tiết course
- `GET /api/user/courses/search` - Tìm kiếm courses
- `GET /api/courses/types` - Lấy danh sách loại courses

#### 2. **Teacher Endpoints** (Authorize Roles = "Teacher")
- `POST /api/courses/teacher/create` - Tạo course mới
- `GET /api/courses/teacher/my-courses` - Lấy danh sách courses của mình
- `GET /api/courses/teacher/{courseId}/detail` - Xem chi tiết course của mình
- `PUT /api/courses/teacher/{courseId}` - Cập nhật course của mình
- `DELETE /api/courses/teacher/{courseId}` - Xóa course của mình
- `POST /api/courses/teacher/{courseId}/students` - Thêm học sinh vào course
- `DELETE /api/courses/teacher/{courseId}/students/{studentId}` - Xóa học sinh khỏi course
- `GET /api/courses/{courseId}/students` - Xem danh sách học sinh trong course
- `GET /api/courses/{courseId}/students/{studentId}` - Xem chi tiết học sinh

#### 3. **Admin Endpoints** (RequirePermission)
- `GET /api/courses/admin/all` - Lấy tất cả courses (cần `Admin.Course.Manage`)
- `POST /api/courses/admin/create` - Tạo course (cần `Admin.Course.Manage`)
- `PUT /api/courses/admin/{courseId}` - Cập nhật course (cần `Admin.Course.Manage`)
- `DELETE /api/courses/admin/{courseId}` - Xóa course (cần `Admin.Course.Manage`)
- `POST /api/courses/admin/{courseId}/students` - Thêm học sinh vào course (cần `Admin.Course.Manage`)
- `DELETE /api/courses/admin/{courseId}/students/{studentId}` - Xóa học sinh khỏi course (cần `Admin.Course.Manage`)

#### 4. **Student Endpoints** (Authorize Roles = "Student")
- `POST /api/user/enrollments/course` - Đăng ký course
- `DELETE /api/user/enrollments/course/{courseId}` - Hủy đăng ký course
- `GET /api/user/enrollments/my-courses` - Lấy danh sách courses đã đăng ký

---

## 🔒 RLS POLICIES HIỆN TẠI CHO COURSES

### 1. **SuperAdmin Policy**
```sql
CREATE POLICY courses_policy_superadmin_all
ON "Courses" FOR ALL
USING (app.is_superadmin());
```
**Chức năng:** Toàn quyền (SELECT, INSERT, UPDATE, DELETE)  
**✅ ĐÚNG**

---

### 2. **Admin Policies** (Permission-aware)
```sql
-- SELECT
CREATE POLICY courses_policy_admin_select
ON "Courses" FOR SELECT
USING (app.user_has_permission('Admin.Course.Manage'));

-- INSERT
CREATE POLICY courses_policy_admin_insert
ON "Courses" FOR INSERT
WITH CHECK (app.user_has_permission('Admin.Course.Manage'));

-- UPDATE
CREATE POLICY courses_policy_admin_update
ON "Courses" FOR UPDATE
USING (app.user_has_permission('Admin.Course.Manage'));

-- DELETE
CREATE POLICY courses_policy_admin_delete
ON "Courses" FOR DELETE
USING (app.user_has_permission('Admin.Course.Manage'));
```
**Chức năng:** Admin cần permission `Admin.Course.Manage` để thao tác courses  
**✅ ĐÚNG** - Defense in depth (App + DB đều check permission)

---

### 3. **Teacher Policy**
```sql
CREATE POLICY courses_policy_teacher_all_own
ON "Courses" FOR ALL
USING (
    app.user_has_role('Teacher')
    AND "TeacherId" = app.current_user_id()
);
```
**Chức năng:** Teacher toàn quyền với courses của chính mình (`TeacherId = current_user_id`)  
**✅ ĐÚNG** - Cho phép:
- ✅ CREATE: Tạo course mới (với `TeacherId = current_user_id()`)
- ✅ SELECT: Xem courses của mình
- ✅ UPDATE: Sửa courses của mình
- ✅ DELETE: Xóa courses của mình

**Lưu ý:** Policy này chỉ áp dụng cho bảng `Courses`. Teacher thêm nội dung (lessons/modules) được kiểm soát bởi RLS policies của bảng `Lessons` và `Modules`.

---

### 4. **Student Policies**
```sql
-- Xem system courses để browse và enroll
CREATE POLICY courses_policy_student_select_system
ON "Courses" FOR SELECT
USING (
    app.user_has_role('Student')
    AND "Type" = 1
);

-- Xem courses đã đăng ký
CREATE POLICY courses_policy_student_select_enrolled
ON "Courses" FOR SELECT
USING (
    app.user_has_role('Student')
    AND EXISTS (
        SELECT 1
        FROM "UserCourses"
        WHERE "UserCourses"."CourseId" = "Courses"."CourseId"
        AND "UserCourses"."UserId" = app.current_user_id()
    )
);
```
**Chức năng:**
- ✅ Xem system courses (Type = 1) để browse và enroll
- ✅ Xem courses đã đăng ký (qua `UserCourses`)

**✅ ĐÚNG**

---

### 5. **Guest Policy**
```sql
CREATE POLICY courses_policy_guest_select_system
ON "Courses" FOR SELECT
USING (
    "Type" = 1
    AND app.current_user_id() IS NULL
);
```
**Chức năng:** Guest chỉ xem system courses (Type = 1)  
**✅ ĐÚNG** - Cho phép:
- ✅ Xem danh sách system courses
- ✅ Xem chi tiết system courses
- ❌ KHÔNG thể xem teacher courses
- ❌ KHÔNG thể xem lessons/modules (phải đăng ký trước)

---

## 🔒 RLS POLICIES CHO USERCOURSES (Quản lý học sinh)

### 1. **SuperAdmin Policy**
```sql
CREATE POLICY usercourses_policy_superadmin_all
ON "UserCourses" FOR ALL
USING (app.is_superadmin());
```
**✅ ĐÚNG**

---

### 2. **Admin Policy**
```sql
CREATE POLICY usercourses_policy_admin_all
ON "UserCourses" FOR ALL
USING (app.user_has_permission('Admin.Course.Manage'));
```
**Chức năng:** Admin có permission `Admin.Course.Manage` → toàn quyền trên UserCourses  
**✅ ĐÚNG**

---

### 3. **Teacher Policies** (Thêm/Xóa học sinh)
```sql
-- INSERT: Teacher thêm học sinh vào courses của mình
CREATE POLICY usercourses_policy_teacher_insert_own_courses
ON "UserCourses" FOR INSERT
WITH CHECK (
    app.user_has_role('Teacher')
    AND EXISTS (
        SELECT 1
        FROM "Courses"
        WHERE "Courses"."CourseId" = "UserCourses"."CourseId"
        AND "Courses"."TeacherId" = app.current_user_id()
    )
);

-- DELETE: Teacher xóa học sinh khỏi courses của mình
CREATE POLICY usercourses_policy_teacher_delete_own_courses
ON "UserCourses" FOR DELETE
USING (
    app.user_has_role('Teacher')
    AND EXISTS (
        SELECT 1
        FROM "Courses"
        WHERE "Courses"."CourseId" = "UserCourses"."CourseId"
        AND "Courses"."TeacherId" = app.current_user_id()
    )
);
```
**Chức năng:**
- ✅ Teacher có thể thêm học sinh vào courses của mình
- ✅ Teacher có thể xóa học sinh khỏi courses của mình
- ❌ Teacher KHÔNG thể xem danh sách học sinh? → **CẦN KIỂM TRA**

**⚠️ VẤN ĐỀ:** Teacher cần SELECT policy để xem danh sách học sinh trong courses của mình!

---

### 4. **Student Policy**
```sql
CREATE POLICY usercourses_policy_student_all_own
ON "UserCourses" FOR ALL
USING (
    app.user_has_role('Student')
    AND "UserId" = app.current_user_id()
);
```
**Chức năng:** Student chỉ thao tác trên enrollment của chính mình  
**✅ ĐÚNG**

---

## ⚠️ VẤN ĐỀ PHÁT HIỆN

### 1. **Teacher không có SELECT policy cho UserCourses**

**Endpoint:** `GET /api/courses/{courseId}/students`  
**Service:** `GetUsersByCourseIdPagedAsync`  
**Repository:** `GetEnrolledUsers(int courseId)`

**Vấn đề:** Teacher cần xem danh sách học sinh trong courses của mình, nhưng không có SELECT policy cho `UserCourses`.

**Giải pháp:** Thêm SELECT policy cho Teacher:
```sql
CREATE POLICY usercourses_policy_teacher_select_own_courses
ON "UserCourses" FOR SELECT
USING (
    app.user_has_role('Teacher')
    AND EXISTS (
        SELECT 1
        FROM "Courses"
        WHERE "Courses"."CourseId" = "UserCourses"."CourseId"
        AND "Courses"."TeacherId" = app.current_user_id()
    )
);
```

---

## ✅ KẾT LUẬN

### RLS Policies cho Courses - ĐÃ ĐÚNG:
1. ✅ SuperAdmin: Toàn quyền
2. ✅ Admin: Permission-based (defense in depth)
3. ✅ Teacher: Ownership-based (toàn quyền với courses của mình)
4. ✅ Student: System courses + Enrolled courses
5. ✅ Guest: Chỉ system courses

### RLS Policies cho UserCourses - CẦN SỬA:
1. ✅ SuperAdmin: Toàn quyền
2. ✅ Admin: Permission-based
3. ⚠️ **Teacher: THIẾU SELECT policy** → Cần thêm để xem danh sách học sinh
4. ✅ Teacher: INSERT/DELETE policies đã đúng
5. ✅ Student: Ownership-based

### Teacher thêm nội dung (Lessons/Modules):
- ✅ RLS policies cho `Lessons` và `Modules` đã đúng
- ✅ Teacher chỉ thêm vào courses của mình (qua JOIN với Courses)

---

## 🔧 CẦN SỬA

**Thêm SELECT policy cho Teacher trên UserCourses:**
```sql
CREATE POLICY usercourses_policy_teacher_select_own_courses
ON "UserCourses" FOR SELECT
USING (
    app.user_has_role('Teacher')
    AND EXISTS (
        SELECT 1
        FROM "Courses"
        WHERE "Courses"."CourseId" = "UserCourses"."CourseId"
        AND "Courses"."TeacherId" = app.current_user_id()
    )
);
```


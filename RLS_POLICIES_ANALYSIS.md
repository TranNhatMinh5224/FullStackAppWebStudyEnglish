# PHÂN TÍCH ĐẦY ĐỦ RLS POLICIES CHO TẤT CẢ TABLES

## 📊 TỔNG QUAN TABLES TRONG HỆ THỐNG

### Tables đã có RLS (7 tables):
1. ✅ **Courses** - Đã có policies
2. ✅ **UserCourses** - Đã có policies
3. ✅ **Lessons** - Đã có policies
4. ✅ **Modules** - Đã có policies
5. ✅ **EssaySubmissions** - Đã có policies
6. ✅ **QuizAttempts** - Đã có policies
7. ✅ **Payments** - Đã có policies

### Tables chưa có RLS (cần phân tích):

#### 1. **Users** - CẦN RLS
**Lý do:** Chứa thông tin nhạy cảm của users
**Operations từ Repository:**
- `GetByIdAsync`, `GetUserByEmailAsync`, `GetUserByPhoneNumberAsync` - SELECT
- `GetAllUsersAsync`, `GetUsersByRoleAsync` - SELECT
- `GetAllUsersPagedAsync`, `GetUsersByCourseIdPagedAsync` - SELECT
- `AddUserAsync` - INSERT
- `UpdateUserAsync` - UPDATE
- `DeleteUserAsync` - DELETE

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.User.Manage`)
- User: Chỉ xem/sửa thông tin của chính mình
- Teacher/Student: Chỉ xem thông tin của chính mình

---

#### 2. **Roles, Permissions, RolePermissions** - KHÔNG CẦN RLS
**Lý do:** Master data, chỉ SuperAdmin/Admin quản lý qua application layer
**Note:** Có thể thêm RLS cho defense in depth nhưng không bắt buộc

---

#### 3. **Lectures** - CẦN RLS
**Lý do:** Thuộc về Modules → cần filter theo ownership
**Operations:**
- SELECT: Student xem lectures của enrolled courses, Teacher xem của own courses
- INSERT/UPDATE/DELETE: Teacher chỉ thao tác trên lectures của own courses

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Lesson.Manage`)
- Teacher: Ownership-based (qua Module → Lesson → Course)
- Student: Xem lectures của enrolled courses

---

#### 4. **FlashCards** - CẦN RLS
**Lý do:** Thuộc về Modules → cần filter theo ownership
**Operations:**
- SELECT: Student xem flashcards của enrolled courses
- INSERT/UPDATE/DELETE: Teacher chỉ thao tác trên flashcards của own courses

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Content.Manage`)
- Teacher: Ownership-based (qua Module → Lesson → Course)
- Student: Xem flashcards của enrolled courses

---

#### 5. **Assessments** - CẦN RLS
**Lý do:** Thuộc về Modules → cần filter theo ownership
**Operations:**
- SELECT: Student xem assessments của enrolled courses
- INSERT/UPDATE/DELETE: Teacher chỉ thao tác trên assessments của own courses

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Content.Manage`)
- Teacher: Ownership-based (qua Module → Lesson → Course)
- Student: Xem assessments của enrolled courses

---

#### 6. **Quizzes, QuizSections, QuizGroups** - CẦN RLS
**Lý do:** Thuộc về Assessments → Modules → cần filter theo ownership
**Operations:**
- SELECT: Student xem quizzes của enrolled courses
- INSERT/UPDATE/DELETE: Teacher chỉ thao tác trên quizzes của own courses

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Content.Manage`)
- Teacher: Ownership-based (qua Assessment → Module → Lesson → Course)
- Student: Xem quizzes của enrolled courses

---

#### 7. **Essays** - CẦN RLS
**Lý do:** Thuộc về Assessments → Modules → cần filter theo ownership
**Operations:**
- SELECT: Student xem essays của enrolled courses
- INSERT/UPDATE/DELETE: Teacher chỉ thao tác trên essays của own courses

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Content.Manage`)
- Teacher: Ownership-based (qua Assessment → Module → Lesson → Course)
- Student: Xem essays của enrolled courses

---

#### 8. **Questions, AnswerOptions** - CẦN RLS
**Lý do:** Thuộc về Quizzes → cần filter theo ownership
**Operations:**
- SELECT: Student xem questions của enrolled courses
- INSERT/UPDATE/DELETE: Teacher chỉ thao tác trên questions của own courses

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Content.Manage`)
- Teacher: Ownership-based (qua Quiz → Assessment → Module → Lesson → Course)
- Student: Xem questions của enrolled courses

---

#### 9. **TeacherPackages** - KHÔNG CẦN RLS
**Lý do:** Master data, không có user-specific ownership
**Note:** Chỉ Admin quản lý, Guest/User có thể xem để mua

---

#### 10. **TeacherSubscriptions** - CẦN RLS
**Lý do:** Chứa thông tin subscription của Teacher
**Operations:**
- SELECT: Teacher chỉ xem subscription của chính mình
- INSERT: System/Admin tạo subscription
- UPDATE/DELETE: Admin quản lý

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Package.Manage`)
- Teacher: Chỉ xem subscription của chính mình (`UserId = current_user_id()`)

---

#### 11. **Notifications** - CẦN RLS
**Lý do:** User-specific data
**Operations:**
- SELECT: User chỉ xem notifications của chính mình
- INSERT: System tạo notifications
- UPDATE: User đánh dấu đã đọc

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.User.Manage`) - có thể xem tất cả
- User: Chỉ xem notifications của chính mình (`UserId = current_user_id()`)

---

#### 12. **LessonCompletion, ModuleCompletion, CourseProgress** - CẦN RLS
**Lý do:** User-specific progress data
**Operations:**
- SELECT: User chỉ xem progress của chính mình
- INSERT/UPDATE: User chỉ thao tác trên progress của chính mình
- Teacher: Xem progress của students trong own courses

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Lesson.Manage`)
- Teacher: Xem progress của students trong own courses
- Student: Chỉ thao tác trên progress của chính mình (`UserId = current_user_id()`)

---

#### 13. **FlashCardReview** - CẦN RLS
**Lý do:** User-specific review data
**Operations:**
- SELECT/INSERT/UPDATE: User chỉ thao tác trên reviews của chính mình

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Content.Manage`)
- User: Chỉ thao tác trên reviews của chính mình (`UserId = current_user_id()`)

---

#### 14. **PronunciationProgress** - CẦN RLS
**Lý do:** User-specific progress data
**Operations:**
- SELECT/INSERT/UPDATE: User chỉ thao tác trên progress của chính mình

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.Lesson.Manage`)
- User: Chỉ thao tác trên progress của chính mình (`UserId = current_user_id()`)

---

#### 15. **Streak** - CẦN RLS
**Lý do:** User-specific data
**Operations:**
- SELECT/INSERT/UPDATE: User chỉ thao tác trên streak của chính mình

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.User.Manage`)
- User: Chỉ thao tác trên streak của chính mình (`UserId = current_user_id()`)

---

#### 16. **RefreshTokens, PasswordResetTokens, EmailVerificationTokens** - KHÔNG CẦN RLS
**Lý do:** System-managed tokens, không cần user access
**Note:** Chỉ application layer quản lý, không có user-facing operations

---

#### 17. **ExternalLogins** - CẦN RLS
**Lý do:** User-specific authentication data
**Operations:**
- SELECT: User chỉ xem external logins của chính mình
- INSERT/UPDATE/DELETE: System/User quản lý

**Cần policies:**
- SuperAdmin: Toàn quyền
- Admin: Permission-based (`Admin.User.Manage`)
- User: Chỉ thao tác trên external logins của chính mình (`UserId = current_user_id()`)

---

#### 18. **PaymentWebhookQueue** - KHÔNG CẦN RLS
**Lý do:** System-managed queue, không có user access
**Note:** Chỉ background jobs xử lý

---

#### 19. **ActivityLog** - CẦN RLS (Optional)
**Lý do:** Audit log, thường chỉ Admin xem
**Note:** Có thể thêm RLS cho defense in depth

---

#### 20. **AssetFrontend** - KHÔNG CẦN RLS
**Lý do:** Public assets, không có user-specific data
**Note:** Chỉ Admin quản lý, Guest/User có thể xem

---

## 📋 TÓM TẮT: TABLES CẦN THÊM RLS

### Priority 1 (User-specific sensitive data):
1. ✅ **Users** - Cần RLS
2. ✅ **TeacherSubscriptions** - Cần RLS
3. ✅ **Notifications** - Cần RLS
4. ✅ **ExternalLogins** - Cần RLS

### Priority 2 (Content ownership):
5. ✅ **Lectures** - Cần RLS
6. ✅ **FlashCards** - Cần RLS
7. ✅ **Assessments** - Cần RLS
8. ✅ **Quizzes** - Cần RLS
9. ✅ **QuizSections** - Cần RLS
10. ✅ **QuizGroups** - Cần RLS
11. ✅ **Essays** - Cần RLS
12. ✅ **Questions** - Cần RLS
13. ✅ **AnswerOptions** - Cần RLS

### Priority 3 (Progress data):
14. ✅ **LessonCompletion** - Cần RLS
15. ✅ **ModuleCompletion** - Cần RLS
16. ✅ **CourseProgress** - Cần RLS
17. ✅ **FlashCardReview** - Cần RLS
18. ✅ **PronunciationProgress** - Cần RLS
19. ✅ **Streak** - Cần RLS

### Priority 4 (Optional):
20. ⚠️ **ActivityLog** - Optional RLS
21. ⚠️ **Roles, Permissions, RolePermissions** - Optional RLS (defense in depth)

---

## 🔒 TỔNG KẾT RLS POLICIES CẦN THIẾT

### Đã có (7 tables): ✅
- Courses, UserCourses, Lessons, Modules, EssaySubmissions, QuizAttempts, Payments

### Cần thêm (19 tables): ⚠️
- Users, TeacherSubscriptions, Notifications, ExternalLogins
- Lectures, FlashCards, Assessments, Quizzes, QuizSections, QuizGroups, Essays, Questions, AnswerOptions
- LessonCompletion, ModuleCompletion, CourseProgress, FlashCardReview, PronunciationProgress, Streak

### Không cần (5 tables): ❌
- TeacherPackages (master data)
- RefreshTokens, PasswordResetTokens, EmailVerificationTokens (system-managed)
- PaymentWebhookQueue (system-managed)
- AssetFrontend (public data)

---

## 🎯 NEXT STEPS

1. **Tạo RLS policies cho Priority 1 tables** (Users, TeacherSubscriptions, Notifications, ExternalLogins)
2. **Tạo RLS policies cho Priority 2 tables** (Content tables: Lectures, FlashCards, Assessments, etc.)
3. **Tạo RLS policies cho Priority 3 tables** (Progress tables)
4. **Review và test** tất cả policies


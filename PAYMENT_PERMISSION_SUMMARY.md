# TỔNG HỢP PERMISSION - THANH TOÁN & TEACHER PACKAGE

## ✅ PERMISSION STATUS

### 1. TEACHER PACKAGE - ADMIN ENDPOINTS

**Controller:** `AdminTeacherPackageController`  
**Route:** `/api/admin/teacher-packages`  
**Base Authorization:** `[Authorize(Roles = "Admin")]`

#### Endpoints với Permission:

| Endpoint | Method | Permission | Status |
|----------|--------|------------|--------|
| `GET /api/admin/teacher-packages` | GET | `[RequirePermission("Admin.Package.Manage")]` | ✅ |
| `GET /api/admin/teacher-packages/{id}` | GET | `[RequirePermission("Admin.Package.Manage")]` | ✅ |
| `POST /api/admin/teacher-packages` | POST | `[RequirePermission("Admin.Package.Manage")]` | ✅ |
| `PUT /api/admin/teacher-packages/{id}` | PUT | `[RequirePermission("Admin.Package.Manage")]` | ✅ |
| `DELETE /api/admin/teacher-packages/{id}` | DELETE | `[RequirePermission("Admin.Package.Manage")]` | ✅ |

**Kết luận:** ✅ **ĐẦY ĐỦ** - Tất cả Admin endpoints đều có `[RequirePermission("Admin.Package.Manage")]`

---

### 2. TEACHER PACKAGE - USER/GUEST ENDPOINTS

**Controller:** `TeacherPackageController`  
**Route:** `/api/user/teacher-packages`  
**Base Authorization:** `[AllowAnonymous]`

| Endpoint | Method | Authorization | Status |
|----------|--------|---------------|--------|
| `GET /api/user/teacher-packages` | GET | `[AllowAnonymous]` | ✅ |
| `GET /api/user/teacher-packages/{id}` | GET | `[AllowAnonymous]` | ✅ |

**Kết luận:** ✅ **ĐÚNG** - Public endpoints, không cần permission

---

### 3. PAYMENT - STUDENT ENDPOINTS

**Controller:** `PaymentController`  
**Route:** `/api/user/payments`  
**Base Authorization:** `[Authorize(Roles = "Student")]`

| Endpoint | Method | Authorization | Permission | Status |
|----------|--------|---------------|------------|--------|
| `POST /api/user/payments/process` | POST | `[Authorize(Roles = "Student")]` | - | ✅ |
| `POST /api/user/payments/confirm` | POST | `[Authorize(Roles = "Student")]` | - | ✅ |
| `GET /api/user/payments/history` | GET | `[Authorize(Roles = "Student")]` | - | ✅ |
| `GET /api/user/payments/transaction/{paymentId}` | GET | `[Authorize(Roles = "Student")]` | - | ✅ |
| `POST /api/user/payments/payos/create-link/{paymentId}` | POST | `[Authorize(Roles = "Student")]` | - | ✅ |
| `POST /api/user/payments/payos/confirm/{paymentId}` | POST | `[Authorize(Roles = "Student")]` | - | ✅ |
| `GET /api/user/payments/payos/return` | GET | `[AllowAnonymous]` | - | ✅ |
| `POST /api/user/payments/payos/webhook` | POST | `[AllowAnonymous]` | - | ✅ |

**Kết luận:** ✅ **ĐÚNG** - Student endpoints chỉ cần `[Authorize(Roles = "Student")]`, không cần permission

---

### 4. PAYMENT - ADMIN ENDPOINTS (STATISTICS/DASHBOARD)

**Controller:** `AdminStatisticsController`  
**Route:** `/api/admin/statistics`  
**Base Authorization:** `[Authorize(Roles = "Admin")]`

#### Endpoints với Permission:

| Endpoint | Method | Permission | Status |
|----------|--------|------------|--------|
| `GET /api/admin/statistics/overview` | GET | `[RequirePermission("Admin.Revenue.View")]` | ✅ |
| `GET /api/admin/statistics/revenue` | GET | `[RequirePermission("Admin.Revenue.View")]` | ✅ |
| `GET /api/admin/statistics/revenue/chart` | GET | `[RequirePermission("Admin.Revenue.View")]` | ✅ |
| `GET /api/admin/statistics/users` | GET | `[RequirePermission("Admin.User.Manage")]` | ✅ |
| `GET /api/admin/statistics/courses` | GET | `[RequirePermission("Admin.Course.Manage")]` | ✅ |
| `GET /api/admin/statistics/teachers` | GET | `[RequirePermission("Admin.User.Manage")]` | ✅ |
| `GET /api/admin/statistics/students` | GET | `[RequirePermission("Admin.User.Manage")]` | ✅ |

**Kết luận:** ✅ **ĐẦY ĐỦ** - Tất cả Admin statistics endpoints đều có permission phù hợp

**Lưu ý:**
- Revenue statistics endpoints sử dụng `Admin.Revenue.View` (đúng với mục đích xem thống kê)
- Payment management endpoints (xem danh sách, refund, fix lỗi) vẫn chưa có - có thể tạo `AdminPaymentController` với `Admin.Payment.Manage` nếu cần

---

## 📋 PERMISSION DEFINITIONS

### Admin.Package.Manage
- **ID:** 7
- **Category:** Finance
- **DisplayName:** Quản lý gói giáo viên
- **Description:** Tạo, sửa, xóa teacher packages
- **Status:** ✅ Đã seed, đã sử dụng

### Admin.Payment.Manage
- **ID:** 5
- **Category:** Finance
- **DisplayName:** Quản lý thanh toán
- **Description:** Xem payments, hoàn tiền, fix lỗi thanh toán
- **Status:** ✅ Đã seed, RLS đã có, nhưng chưa có Admin endpoints

---

## 🔒 RLS POLICIES

### Payments Table
- **SuperAdmin:** Toàn quyền (`app.is_superadmin()`)
- **Admin:** Permission-based (`app.user_has_permission('Admin.Payment.Manage')`)
- **Student:** Ownership-based (`UserId = app.current_user_id()`)
- **Webhook:** Anonymous access (`current_user_id IS NULL`)

### TeacherPackages Table
- **Không có RLS** (master data, public)

---

## ✅ TỔNG KẾT

### Teacher Package
- ✅ **Admin endpoints:** Đầy đủ permission (`Admin.Package.Manage`)
- ✅ **User/Guest endpoints:** Đúng (AllowAnonymous)

### Payment
- ✅ **Student endpoints:** Đúng (`[Authorize(Roles = "Student")]`)
- ✅ **Webhook/Return:** Đúng (`[AllowAnonymous]`)
- ⚠️ **Admin endpoints:** Chưa có (nhưng permission và RLS đã sẵn sàng)

---

## 🎯 KẾT LUẬN

**Permission cho Teacher Package:** ✅ **ĐẦY ĐỦ**

**Permission cho Payment:**
- ✅ Student endpoints: Đầy đủ
- ⚠️ Admin endpoints: Chưa có (nhưng permission và RLS đã sẵn sàng nếu cần)

**Nếu cần Admin quản lý payments:** Có thể tạo `AdminPaymentController` và sử dụng `[RequirePermission("Admin.Payment.Manage")]` - permission và RLS đã sẵn sàng!


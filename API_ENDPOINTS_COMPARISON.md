# BÁO CÁO SO SÁNH API ENDPOINTS - FRONTEND vs BACKEND

## 📊 TỔNG QUAN

### Backend Controllers
- **User Controllers**: 15 controllers
- **Admin/Teacher Controllers**: 15 controllers
- **Tổng số endpoints**: ~100+ endpoints

### Frontend Services
- **Hiện có**: 1 service (authService.js)
- **Còn thiếu**: ~14 service files

---

## ✅ API ENDPOINTS ĐÃ CÓ TRONG FRONTEND

### 1. Authentication (authService.js) ✅
| Frontend Endpoint | Backend Route | Method | Status |
|------------------|---------------|--------|--------|
| `user/auth/register` | `api/user/auth/register` | POST | ✅ Khớp |
| `user/auth/login` | `api/user/auth/login` | POST | ✅ Khớp |
| `user/auth/profile` | `api/user/auth/profile` | GET | ✅ Khớp |
| `user/auth/profile` | `api/user/auth/profile` | PUT | ✅ Khớp |
| `user/auth/change-password` | `api/user/auth/change-password` | PUT | ✅ Khớp |
| `user/auth/forgot-password` | `api/user/auth/forgot-password` | POST | ✅ Khớp |
| `user/auth/reset-password` | `api/user/auth/set-new-password` | POST | ⚠️ **KHÔNG KHỚP** |

**⚠️ VẤN ĐỀ PHÁT HIỆN:**
- Frontend gọi `reset-password` nhưng backend là `set-new-password`
- Backend có `verify-otp` nhưng frontend chưa có service method riêng
- Backend có `refresh-token` nhưng frontend chưa có service method

---

## ❌ API ENDPOINTS CÒN THIẾU TRONG FRONTEND

### 2. Courses (userCourseService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/user/courses/system-courses` | GET | Lấy danh sách khóa học hệ thống |

### 3. Enrollment (enrollmentService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/user/enroll/course` | POST | Đăng ký khóa học |
| `api/user/enroll/course/{courseId}` | DELETE | Hủy đăng ký khóa học |
| `api/user/enroll/my-courses` | GET | Lấy danh sách khóa học đã đăng ký |
| `api/user/enroll/join-by-class-code` | POST | Tham gia khóa học bằng mã lớp |

### 4. Lessons (lessonService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/user/lessons/course/{courseId}` | GET | Lấy danh sách bài học theo course |
| `api/user/lessons/{lessonId}` | GET | Lấy thông tin bài học theo ID |

### 5. Modules (moduleService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/user/modules/{moduleId}` | GET | Lấy thông tin module với tiến độ |
| `api/user/modules/lesson/{lessonId}` | GET | Lấy tất cả module trong lesson |

### 6. Lectures (lectureService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/userlecture/{lectureId}` | GET | Lấy thông tin lecture theo ID |
| `api/userlecture/module/{moduleId}` | GET | Lấy danh sách lecture theo module |
| `api/userlecture/module/{moduleId}/tree` | GET | Lấy cấu trúc cây lecture |

### 7. Quizzes (quizService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/User/Quizz/{assessmentId}` | GET | Lấy thông tin quiz theo assessment |
| `api/User/quiz/{quizId}` | GET | Lấy thông tin quiz theo ID |

### 8. Quiz Attempts (quizAttemptService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/User/QuizAttempt/start/{quizId}` | POST | Bắt đầu làm quiz |
| `api/User/QuizAttempt/submit/{attemptId}` | POST | Nộp bài quiz |
| `api/User/QuizAttempt/resume/{attemptId}` | GET | Tiếp tục làm quiz |
| `api/User/QuizAttempt/update-answer/{attemptId}` | POST | Cập nhật câu trả lời |

### 9. FlashCards (flashCardService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/user/flashcard/{id}` | GET | Lấy thông tin flashcard |
| `api/user/flashcard/module/{moduleId}` | GET | Lấy danh sách flashcard theo module |
| `api/user/flashcard/search` | GET | Tìm kiếm flashcard |
| `api/user/flashcard/progress/{moduleId}` | GET | Lấy tiến độ học flashcard |
| `api/user/flashcard/reset-progress/{flashCardId}` | POST | Reset tiến độ flashcard |

### 10. Vocabulary Review (vocabularyReviewService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/user/vocabularyreview/due` | GET | Lấy từ cần ôn tập |
| `api/user/vocabularyreview/new` | GET | Lấy từ mới |
| `api/user/vocabularyreview/start/{flashCardId}` | POST | Bắt đầu ôn tập |
| `api/user/vocabularyreview/submit/{reviewId}` | POST | Nộp kết quả ôn tập |
| `api/user/vocabularyreview/stats` | GET | Thống kê ôn tập |
| `api/user/vocabularyreview/recent` | GET | Lấy từ đã ôn gần đây |
| `api/user/vocabularyreview/reset/{flashCardId}` | POST | Reset ôn tập |

### 11. Assessments (assessmentService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/user/Assessment/module/{moduleId}` | GET | Lấy danh sách assessment theo module |
| `api/user/Assessment/{assessmentId}` | GET | Lấy thông tin assessment |

### 12. Essays (essayService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/User/Essay/{essayId}` | GET | Lấy thông tin essay |
| `api/User/Essay/assessment/{assessmentId}` | GET | Lấy essay theo assessment |

### 13. Essay Submissions (essaySubmissionService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/User/EssaySubmission/submit` | POST | Nộp bài essay |
| `api/User/EssaySubmission/{submissionId}` | GET | Lấy thông tin submission |
| `api/User/EssaySubmission/my-submissions` | GET | Lấy danh sách submission của user |
| `api/User/EssaySubmission/submission-status/assessment/{assessmentId}` | GET | Lấy trạng thái submission |
| `api/User/EssaySubmission/update/{submissionId}` | PUT | Cập nhật submission |
| `api/User/EssaySubmission/delete/{submissionId}` | DELETE | Xóa submission |

### 14. Payments (paymentService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/payment/process` | POST | Xử lý thanh toán |
| `api/payment/confirm` | POST | Xác nhận thanh toán |

### 15. Teacher Packages (teacherPackageService.js) ❌
| Backend Route | Method | Mô tả |
|---------------|--------|-------|
| `api/user/teacher-packages` | GET | Lấy danh sách gói giáo viên |
| `api/user/teacher-packages/{id}` | GET | Lấy thông tin gói giáo viên |

---

## 🔧 CẦN SỬA

### 1. AuthService - Reset Password Endpoint
- **Hiện tại**: `user/auth/reset-password`
- **Cần sửa thành**: `user/auth/set-new-password`

### 2. AuthService - Thiếu methods
- `verifyOTP()` - Gọi `user/auth/verify-otp`
- `refreshToken()` - Gọi `user/auth/refresh-token`

---

## 📝 KẾ HOẠCH THỰC HIỆN

1. ✅ Sửa authService.js (reset-password endpoint + thêm methods)
2. ⏳ Tạo courseService.js
3. ⏳ Tạo enrollmentService.js
4. ⏳ Tạo lessonService.js
5. ⏳ Tạo moduleService.js
6. ⏳ Tạo lectureService.js
7. ⏳ Tạo quizService.js
8. ⏳ Tạo quizAttemptService.js
9. ⏳ Tạo flashCardService.js
10. ⏳ Tạo vocabularyReviewService.js
11. ⏳ Tạo assessmentService.js
12. ⏳ Tạo essayService.js
13. ⏳ Tạo essaySubmissionService.js
14. ⏳ Tạo paymentService.js
15. ⏳ Tạo teacherPackageService.js
16. ⏳ Cập nhật config.js với tất cả endpoints
17. ⏳ Cập nhật index.js để export tất cả services


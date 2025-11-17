# 📚 Tài liệu API: Quiz Answer Format

## Tổng quan

API `POST /api/User/QuizAttempt/update-answer/{attemptId}` nhận `userAnswer` với format khác nhau tùy theo loại câu hỏi.

---

## 📋 Các loại câu hỏi và format UserAnswer

### 1. **MultipleChoice** (Chọn 1 đáp án)
**QuestionType = 0**

**Format:**
```json
{
  "questionId": 1,
  "userAnswer": 1
}
```
hoặc
```json
{
  "questionId": 1,
  "userAnswer": "1"  // String cũng được, sẽ tự động convert
}
```

**Giải thích:** `userAnswer` là `int` - ID của option được chọn.

---

### 2. **MultipleAnswers** (Chọn nhiều đáp án)
**QuestionType = 1**

**Format:**
```json
{
  "questionId": 2,
  "userAnswer": [1, 2, 3]
}
```

**Giải thích:** `userAnswer` là `array of int` - Danh sách ID các option được chọn.

**Lưu ý:** 
- Phải chọn đúng số lượng đáp án đúng mới được điểm
- Thứ tự không quan trọng

---

### 3. **TrueFalse** (Đúng/Sai)
**QuestionType = 2**

**Format:**
```json
{
  "questionId": 3,
  "userAnswer": 1  // ID của option "True" hoặc "False"
}
```

**Giải thích:** `userAnswer` là `int` - ID của option được chọn (True hoặc False).

---

### 4. **FillBlank** (Điền vào chỗ trống)
**QuestionType = 3**

**Format:**
```json
{
  "questionId": 4,
  "userAnswer": "hello world"
}
```

**Giải thích:** `userAnswer` là `string` - Text điền vào chỗ trống.

**Lưu ý:** 
- So sánh không phân biệt hoa thường
- Tự động trim spaces

---

### 5. **Matching** (Ghép nối)
**QuestionType = 4**

**Format:**
```json
{
  "questionId": 5,
  "userAnswer": {
    "1": 2,  // Left option 1 → Right option 2
    "3": 4,  // Left option 3 → Right option 4
    "5": 6   // Left option 5 → Right option 6
  }
}
```

**Giải thích:** `userAnswer` là `Dictionary<int, int>` - Key là leftOptionId, Value là rightOptionId.

**Lưu ý:** 
- Phải ghép đúng tất cả các cặp mới được điểm
- Keys trong JSON là string, nhưng sẽ được convert sang int

---

### 6. **Ordering** (Sắp xếp thứ tự)
**QuestionType = 5**

**Format:**
```json
{
  "questionId": 6,
  "userAnswer": [3, 1, 2, 4]  // Thứ tự: option 3 → option 1 → option 2 → option 4
}
```

**Giải thích:** `userAnswer` là `array of int` - Thứ tự các option từ trên xuống dưới.

**Lưu ý:** 
- Thứ tự phải chính xác 100% mới được điểm
- Không được có option trùng lặp

---

## 🔄 Response Format

### Success Response
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Answer and score updated successfully",
  "data": 1.0  // Điểm của câu hỏi này (0 nếu sai, Points nếu đúng)
}
```

### Error Response
```json
{
  "success": false,
  "statusCode": 404,
  "message": "Question not found",
  "data": 0
}
```

---

## ✅ Best Practices

### 1. **Luôn gửi đúng format theo QuestionType**
- Kiểm tra `question.type` trước khi gửi answer
- Frontend nên validate format trước khi gửi

### 2. **Xử lý Real-time Scoring**
- Gọi API mỗi khi user thay đổi đáp án
- Hiển thị điểm ngay lập tức từ response `data`

### 3. **Xử lý lỗi**
- Kiểm tra `success` trong response
- Hiển thị `message` cho user nếu có lỗi

### 4. **Type Safety (Tùy chọn)**
- Có thể dùng các DTOs riêng trong `UserAnswerDtos.cs`:
  - `SingleChoiceAnswerDto` cho MultipleChoice/TrueFalse
  - `MultipleChoiceAnswerDto` cho MultipleAnswers
  - `FillBlankAnswerDto` cho FillBlank
  - `MatchingAnswerDto` cho Matching
  - `OrderingAnswerDto` cho Ordering

---

## 📝 Ví dụ đầy đủ

### Test với Postman/Thunder Client

**Request:**
```
POST http://localhost:5029/api/User/QuizAttempt/update-answer/7
Content-Type: application/json
Authorization: Bearer {token}
```

**Body (MultipleChoice):**
```json
{
  "questionId": 1,
  "userAnswer": 1
}
```

**Body (MultipleAnswers):**
```json
{
  "questionId": 2,
  "userAnswer": [1, 2, 3]
}
```

**Body (FillBlank):**
```json
{
  "questionId": 4,
  "userAnswer": "hello"
}
```

**Body (Matching):**
```json
{
  "questionId": 5,
  "userAnswer": {
    "1": 2,
    "3": 4
  }
}
```

**Body (Ordering):**
```json
{
  "questionId": 6,
  "userAnswer": [3, 1, 2, 4]
}
```

---

## 🚨 Common Errors

### 1. "Unable to cast object..."
- **Nguyên nhân:** Format `userAnswer` không đúng với QuestionType
- **Giải pháp:** Kiểm tra lại format theo bảng trên

### 2. "Question not found"
- **Nguyên nhân:** `questionId` không tồn tại hoặc không thuộc quiz này
- **Giải pháp:** Kiểm tra lại `questionId` từ response start quiz

### 3. "Attempt not found or not in progress"
- **Nguyên nhân:** Attempt đã submit hoặc không tồn tại
- **Giải pháp:** Kiểm tra lại `attemptId` và status

---

**Last Updated:** 2025-11-17


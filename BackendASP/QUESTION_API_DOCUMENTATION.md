# Question & Answer API Documentation

## 📋 Tổng quan

API này quản lý **Questions** (Câu hỏi) và **Answer Options** (Đáp án) theo đúng cách code của bạn:
- ✅ Sử dụng `ServiceResponse<T>` wrapper
- ✅ Logging với ILogger
- ✅ Validation với FluentValidation
- ✅ AutoMapper cho mapping
- ✅ Transaction-safe bulk operations
- ✅ **Bulk create gộp cả Question + Answer Options** (1 lần call, tạo nhiều câu hỏi kèm đáp án)

---

## 🚀 Endpoints

### 1. Lấy câu hỏi theo ID
```http
GET /api/Question/{questionId}
Authorization: Bearer {token}
```

**Response Success:**
```json
{
  "success": true,
  "message": "Lấy thông tin câu hỏi thành công.",
  "data": {
    "questionId": 1,
    "type": 0,
    "stemText": "What is the capital of France?",
    "stemHtml": null,
    "quizGroupId": 1,
    "quizSectionId": 1,
    "points": 10,
    "scoring": 0,
    "correctAnswersJson": "[0]",
    "metadataJson": "{\"difficulty\":\"easy\"}",
    "explanation": "Paris is the capital of France.",
    "mediaUrl": null,
    "mediaType": null,
    "createdAt": "2025-11-12T10:00:00Z",
    "updatedAt": "2025-11-12T10:00:00Z",
    "options": [
      {
        "answerOptionId": 1,
        "questionId": 1,
        "text": "Paris",
        "isCorrect": true,
        "mediaUrl": null,
        "mediaType": null,
        "orderIndex": 0,
        "feedback": "Correct!"
      },
      {
        "answerOptionId": 2,
        "questionId": 1,
        "text": "London",
        "isCorrect": false,
        "mediaUrl": null,
        "mediaType": null,
        "orderIndex": 1,
        "feedback": "London is the capital of UK."
      }
    ]
  },
  "statusCode": 200
}
```

---

### 2. Lấy danh sách câu hỏi theo QuizGroup
```http
GET /api/Question/quiz-group/{quizGroupId}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Lấy danh sách 5 câu hỏi thành công.",
  "data": [
    {
      "questionId": 1,
      "stemText": "Question 1",
      "options": [...]
    }
  ],
  "statusCode": 200
}
```

---

### 3. Lấy danh sách câu hỏi theo QuizSection
```http
GET /api/Question/quiz-section/{quizSectionId}
Authorization: Bearer {token}
```

---

### 4. Tạo câu hỏi mới (kèm đáp án)
```http
POST /api/Question/create
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "type": 0,
  "stemText": "What is the capital of France?",
  "stemHtml": null,
  "quizGroupId": 1,
  "quizSectionId": 1,
  "points": 10,
  "scoring": 0,
  "correctAnswersJson": "[0]",
  "metadataJson": "{\"difficulty\":\"easy\"}",
  "explanation": "Paris is the capital of France.",
  "mediaUrl": null,
  "mediaType": null,
  "options": [
    {
      "text": "Paris",
      "isCorrect": true,
      "mediaUrl": null,
      "mediaType": null,
      "orderIndex": 0,
      "feedback": "Correct! Paris is the capital of France."
    },
    {
      "text": "London",
      "isCorrect": false,
      "mediaUrl": null,
      "mediaType": null,
      "orderIndex": 1,
      "feedback": "London is the capital of the UK."
    }
  ]
}
```

**Response Success:**
```json
{
  "success": true,
  "message": "Tạo câu hỏi thành công.",
  "data": {
    "questionId": 101,
    "stemText": "What is the capital of France?",
    "options": [...]
  },
  "statusCode": 201
}
```

---

### 5. 🔥 Tạo hàng loạt câu hỏi (Bulk Create) - GỘP CẢ QUESTION + ANSWER
```http
POST /api/Question/bulk-create
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:** (Tạo nhiều câu hỏi cùng lúc, mỗi câu có nhiều đáp án)
```json
{
  "questions": [
    {
      "type": 0,
      "stemText": "What is the capital of France?",
      "stemHtml": null,
      "quizGroupId": 1,
      "quizSectionId": 1,
      "points": 10,
      "scoring": 0,
      "correctAnswersJson": "[0]",
      "metadataJson": "{\"difficulty\":\"easy\",\"tags\":[\"geography\"]}",
      "explanation": "Paris is the capital and largest city of France.",
      "mediaUrl": null,
      "mediaType": null,
      "options": [
        {
          "text": "Paris",
          "isCorrect": true,
          "mediaUrl": null,
          "mediaType": null,
          "orderIndex": 0,
          "feedback": "Correct! Paris is the capital of France."
        },
        {
          "text": "London",
          "isCorrect": false,
          "mediaUrl": null,
          "mediaType": null,
          "orderIndex": 1,
          "feedback": "London is the capital of the UK."
        },
        {
          "text": "Berlin",
          "isCorrect": false,
          "mediaUrl": null,
          "mediaType": null,
          "orderIndex": 2,
          "feedback": "Berlin is the capital of Germany."
        }
      ]
    },
    {
      "type": 1,
      "stemText": "Select all programming languages:",
      "stemHtml": null,
      "quizGroupId": 1,
      "quizSectionId": 1,
      "points": 15,
      "scoring": 1,
      "correctAnswersJson": "[0,1,3]",
      "metadataJson": "{\"difficulty\":\"medium\",\"tags\":[\"programming\"]}",
      "explanation": "Python, Java, and C# are programming languages.",
      "mediaUrl": null,
      "mediaType": null,
      "options": [
        {
          "text": "Python",
          "isCorrect": true,
          "mediaUrl": null,
          "mediaType": null,
          "orderIndex": 0,
          "feedback": "Python is a programming language."
        },
        {
          "text": "Java",
          "isCorrect": true,
          "mediaUrl": null,
          "mediaType": null,
          "orderIndex": 1,
          "feedback": "Java is a programming language."
        },
        {
          "text": "HTML",
          "isCorrect": false,
          "mediaUrl": null,
          "mediaType": null,
          "orderIndex": 2,
          "feedback": "HTML is a markup language."
        },
        {
          "text": "C#",
          "isCorrect": true,
          "mediaUrl": null,
          "mediaType": null,
          "orderIndex": 3,
          "feedback": "C# is a programming language."
        }
      ]
    }
  ]
}
```

**Response Success:**
```json
{
  "success": true,
  "message": "Tạo thành công 2 câu hỏi với tất cả đáp án.",
  "data": {
    "createdQuestionIds": [101, 102]
  },
  "statusCode": 201
}
```

**Lợi ích Bulk Create:**
- ✅ **Performance**: 1 transaction thay vì N transactions
- ✅ **Atomic**: Tất cả thành công hoặc rollback hết
- ✅ **Gộp cả Answer Options**: Không cần call riêng để tạo đáp án
- ✅ **Auto-generate IDs**: Trả về danh sách QuestionId đã tạo

---

### 6. Cập nhật câu hỏi
```http
PUT /api/Question/update/{questionId}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:** (giống QuestionCreateDto)

---

### 7. Xóa câu hỏi
```http
DELETE /api/Question/delete/{questionId}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Xóa câu hỏi thành công.",
  "data": true,
  "statusCode": 200
}
```

---

## 📊 Enums

### TypeQuestion
```csharp
0 = MultipleChoice    // Chọn 1 đáp án đúng
1 = MultipleAnswers   // Chọn nhiều đáp án đúng
2 = TrueFalse        // Đúng/Sai
3 = FillBlank        // Điền vào chỗ trống
4 = Matching         // Nối từ/cụm từ
5 = Ordering         // Sắp xếp thứ tự
6 = ShortAnswer      // Câu trả lời ngắn
7 = ImageChoice      // Chọn hình ảnh
```

### ScoringStrategy
```csharp
0 = AllOrNothing    // Phải đúng hết mới được điểm
1 = PartialCredit   // Được điểm theo tỷ lệ đúng
```

---

## ✅ Validation Rules

### Question:
- `stemText`: Required, max 2000 chars
- `points`: > 0, ≤ 1000
- `quizGroupId`: > 0
- `quizSectionId`: > 0
- `options`: ≥ 2 đáp án cho MultipleChoice/MultipleAnswers
- `options`: Ít nhất 1 đáp án đúng
- `options`: MultipleChoice chỉ có 1 đáp án đúng

### Answer Option:
- `text`: Required, max 1000 chars
- `orderIndex`: ≥ 0

### Bulk:
- `questions`: Not empty
- `questions`: Max 100 câu/lần

---

## 🏗️ Architecture

```
Controller (QuestionController)
    ↓ ServiceResponse<T>
Service (QuestionService)
    ↓ Entity
Repository (QuestionRepository)
    ↓ EF Core
Database (PostgreSQL)
```

**Key Features:**
- ✅ **ServiceResponse wrapper**: Consistent response format
- ✅ **Logging**: Track tất cả operations
- ✅ **Validation**: FluentValidation trước khi vào service
- ✅ **Transaction**: Bulk operations dùng database transaction
- ✅ **Auto-mapping**: DTO ↔ Entity với AutoMapper
- ✅ **Cascade insert**: EF Core tự động insert Options khi insert Question

---

## 🧪 Testing với Postman/Swagger

1. **Login** để lấy token
2. **Tạo QuizSection & QuizGroup** trước
3. **Bulk create questions** với file `sample-bulk-questions.json`
4. **Verify**: GET các câu hỏi vừa tạo

---

## 📝 Notes

- **Bulk create tự động gộp cả Question + Answer Options** trong 1 transaction
- EF Core tự động insert cascade cho navigation properties
- Tất cả IDs được auto-generate sau SaveChanges
- Dùng transaction để đảm bảo data consistency
- Repository không có SaveChanges riêng, tất cả thông qua service layer

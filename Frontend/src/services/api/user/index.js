// Main export file for user API
import { AuthService } from './authService.js';
import { CourseService } from './courseService.js';
import { EnrollmentService } from './enrollmentService.js';
import { LessonService } from './lessonService.js';
import { ModuleService } from './moduleService.js';
import { LectureService } from './lectureService.js';
import { QuizService } from './quizService.js';
import { QuizAttemptService } from './quizAttemptService.js';
import { FlashCardService } from './flashCardService.js';
import { VocabularyReviewService } from './vocabularyReviewService.js';
import { AssessmentService } from './assessmentService.js';
import { EssayService } from './essayService.js';
import { EssaySubmissionService } from './essaySubmissionService.js';
import { PaymentService } from './paymentService.js';
import { TeacherPackageService } from './teacherPackageService.js';

// Export config and utilities
export { BASE_URL, API_ENDPOINTS } from './config.js';
export { TokenManager } from './tokenManager.js';
export { httpClient } from './httpClient.js';

// Export all services
export { AuthService } from './authService.js';
export { CourseService } from './courseService.js';
export { EnrollmentService } from './enrollmentService.js';
export { LessonService } from './lessonService.js';
export { ModuleService } from './moduleService.js';
export { LectureService } from './lectureService.js';
export { QuizService } from './quizService.js';
export { QuizAttemptService } from './quizAttemptService.js';
export { FlashCardService } from './flashCardService.js';
export { VocabularyReviewService } from './vocabularyReviewService.js';
export { AssessmentService } from './assessmentService.js';
export { EssayService } from './essayService.js';
export { EssaySubmissionService } from './essaySubmissionService.js';
export { PaymentService } from './paymentService.js';
export { TeacherPackageService } from './teacherPackageService.js';

// For backward compatibility, export AuthService as AuthAPI
export { AuthService as AuthAPI } from './authService.js';

// Default export (for backward compatibility)
export default AuthService;
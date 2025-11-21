// API configuration and base setup
export const BASE_URL = 'http://localhost:5029/api';

// API Endpoints
export const API_ENDPOINTS = {
  AUTH: {
    REGISTER: 'user/auth/register',
    LOGIN: 'user/auth/login',
    PROFILE: 'user/auth/profile',
    UPDATE_PROFILE: 'user/auth/profile',
    CHANGE_PASSWORD: 'user/auth/change-password',
    FORGOT_PASSWORD: 'user/auth/forgot-password',
    VERIFY_OTP: 'user/auth/verify-otp',
    SET_NEW_PASSWORD: 'user/auth/set-new-password',
    REFRESH_TOKEN: 'user/auth/refresh-token'
  },
  COURSES: {
    SYSTEM_COURSES: 'user/courses/system-courses'
  },
  ENROLLMENT: {
    ENROLL: 'user/enroll/course',
    UNENROLL: (courseId) => `user/enroll/course/${courseId}`,
    MY_COURSES: 'user/enroll/my-courses',
    JOIN_BY_CLASS_CODE: 'user/enroll/join-by-class-code'
  },
  LESSONS: {
    BY_COURSE: (courseId) => `user/lessons/course/${courseId}`,
    BY_ID: (lessonId) => `user/lessons/${lessonId}`
  },
  MODULES: {
    BY_ID: (moduleId) => `user/modules/${moduleId}`,
    BY_LESSON: (lessonId) => `user/modules/lesson/${lessonId}`
  },
  LECTURES: {
    BY_ID: (lectureId) => `UserLecture/${lectureId}`,
    BY_MODULE: (moduleId) => `UserLecture/module/${moduleId}`,
    TREE_BY_MODULE: (moduleId) => `UserLecture/module/${moduleId}/tree`
  },
  QUIZZES: {
    BY_ASSESSMENT: (assessmentId) => `User/Quizz/${assessmentId}`,
    BY_ID: (quizId) => `User/quiz/${quizId}`
  },
  QUIZ_ATTEMPTS: {
    START: (quizId) => `User/QuizAttempt/start/${quizId}`,
    SUBMIT: (attemptId) => `User/QuizAttempt/submit/${attemptId}`,
    RESUME: (attemptId) => `User/QuizAttempt/resume/${attemptId}`,
    UPDATE_ANSWER: (attemptId) => `User/QuizAttempt/update-answer/${attemptId}`
  },
  FLASHCARDS: {
    BY_ID: (id) => `user/UserFlashCard/${id}`,
    BY_MODULE: (moduleId) => `user/UserFlashCard/module/${moduleId}`,
    SEARCH: 'user/UserFlashCard/search',
    PROGRESS: (moduleId) => `user/UserFlashCard/progress/${moduleId}`,
    RESET_PROGRESS: (flashCardId) => `user/UserFlashCard/reset-progress/${flashCardId}`
  },
  VOCABULARY_REVIEW: {
    DUE: 'user/VocabularyReview/due',
    NEW: 'user/VocabularyReview/new',
    START: (flashCardId) => `user/VocabularyReview/start/${flashCardId}`,
    SUBMIT: (reviewId) => `user/VocabularyReview/submit/${reviewId}`,
    STATS: 'user/VocabularyReview/stats',
    RECENT: 'user/VocabularyReview/recent',
    RESET: (flashCardId) => `user/VocabularyReview/reset/${flashCardId}`
  },
  ASSESSMENTS: {
    BY_MODULE: (moduleId) => `user/Assessment/module/${moduleId}`,
    BY_ID: (assessmentId) => `user/Assessment/${assessmentId}`
  },
  ESSAYS: {
    BY_ID: (essayId) => `User/Essay/${essayId}`,
    BY_ASSESSMENT: (assessmentId) => `User/Essay/assessment/${assessmentId}`
  },
  ESSAY_SUBMISSIONS: {
    SUBMIT: 'User/EssaySubmission/submit',
    BY_ID: (submissionId) => `User/EssaySubmission/${submissionId}`,
    MY_SUBMISSIONS: 'User/EssaySubmission/my-submissions',
    STATUS_BY_ASSESSMENT: (assessmentId) => `User/EssaySubmission/submission-status/assessment/${assessmentId}`,
    UPDATE: (submissionId) => `User/EssaySubmission/update/${submissionId}`,
    DELETE: (submissionId) => `User/EssaySubmission/delete/${submissionId}`
  },
  PAYMENTS: {
    PROCESS: 'payment/process',
    CONFIRM: 'payment/confirm'
  },
  TEACHER_PACKAGES: {
    LIST: 'user/teacher-packages',
    BY_ID: (id) => `user/teacher-packages/${id}`
  },
  ADMIN: {
    USERS: {
      ALL: 'admin/auth/users',
      TEACHERS: 'admin/auth/teachers',
      STUDENTS_BY_COURSES: 'admin/auth/getall-students-by-all-courses',
      BLOCKED: 'admin/auth/list-blocked-accounts',
      BLOCK: (userId) => `admin/auth/block-account/${userId}`,
      UNBLOCK: (userId) => `admin/auth/unblock-account/${userId}`
    },
    COURSES: {
      ALL: 'admin/all',
      CREATE: 'admin/create',
      UPDATE: (courseId) => `admin/${courseId}`,
      DELETE: (courseId) => `admin/${courseId}`,
      USERS_BY_COURSE: (courseId) => `getusersbycourse/${courseId}`
    },
    TEACHER_PACKAGES: {
      ALL: 'admin/teacher-packages',
      BY_ID: (id) => `admin/teacher-packages/${id}`,
      CREATE: 'admin/teacher-packages',
      UPDATE: (id) => `admin/teacher-packages/Update-Teacher-Package${id}`,
      DELETE: (id) => `admin/teacher-packages/${id}`
    },
    ASSESSMENTS: {
      CREATE: 'AdminAndTeacher/Assessment/AdminAssessmentController/create',
      BY_MODULE: (moduleId) => `AdminAndTeacher/Assessment/AdminAssessmentController/module/${moduleId}`,
      BY_ID: (assessmentId) => `AdminAndTeacher/Assessment/AdminAssessmentController/${assessmentId}`,
      UPDATE: (assessmentId) => `AdminAndTeacher/Assessment/AdminAssessmentController/${assessmentId}`,
      DELETE: (assessmentId) => `AdminAndTeacher/Assessment/AdminAssessmentController/${assessmentId}`
    }
  }
};
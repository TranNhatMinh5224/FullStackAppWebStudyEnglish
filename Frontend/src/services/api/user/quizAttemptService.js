import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Quiz Attempt API methods
export const QuizAttemptService = {
  // Start a quiz attempt
  startQuiz: async (quizId) => {
    try {
      return await httpClient.post(API_ENDPOINTS.QUIZ_ATTEMPTS.START(quizId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Submit a quiz attempt
  submitQuiz: async (attemptId, answers) => {
    try {
      return await httpClient.post(API_ENDPOINTS.QUIZ_ATTEMPTS.SUBMIT(attemptId), { answers });
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Resume a quiz attempt
  resumeQuiz: async (attemptId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.QUIZ_ATTEMPTS.RESUME(attemptId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Update answer in a quiz attempt
  updateAnswer: async (attemptId, questionId, answer) => {
    try {
      return await httpClient.post(API_ENDPOINTS.QUIZ_ATTEMPTS.UPDATE_ANSWER(attemptId), {
        questionId,
        answer
      });
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


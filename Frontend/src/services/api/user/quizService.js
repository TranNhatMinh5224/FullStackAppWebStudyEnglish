import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Quiz API methods
export const QuizService = {
  // Get quiz information by assessment ID
  getQuizByAssessmentId: async (assessmentId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.QUIZZES.BY_ASSESSMENT(assessmentId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get quiz by ID
  getQuizById: async (quizId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.QUIZZES.BY_ID(quizId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


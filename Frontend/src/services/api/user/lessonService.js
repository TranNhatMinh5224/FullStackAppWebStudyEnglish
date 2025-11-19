import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Lesson API methods
export const LessonService = {
  // Get lessons by course ID
  getLessonsByCourseId: async (courseId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.LESSONS.BY_COURSE(courseId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get lesson by ID
  getLessonById: async (lessonId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.LESSONS.BY_ID(lessonId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


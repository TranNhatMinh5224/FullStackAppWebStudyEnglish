import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Lecture API methods
export const LectureService = {
  // Get lecture by ID
  getLectureById: async (lectureId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.LECTURES.BY_ID(lectureId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get lectures by module ID
  getLecturesByModule: async (moduleId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.LECTURES.BY_MODULE(moduleId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get lecture tree by module ID
  getLectureTree: async (moduleId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.LECTURES.TREE_BY_MODULE(moduleId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


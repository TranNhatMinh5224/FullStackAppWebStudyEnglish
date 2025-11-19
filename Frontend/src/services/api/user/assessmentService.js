import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Assessment API methods
export const AssessmentService = {
  // Get assessments by module ID
  getAssessmentsByModule: async (moduleId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ASSESSMENTS.BY_MODULE(moduleId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get assessment by ID
  getAssessmentById: async (assessmentId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ASSESSMENTS.BY_ID(assessmentId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


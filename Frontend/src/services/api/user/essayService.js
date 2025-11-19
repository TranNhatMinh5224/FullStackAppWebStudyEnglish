import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Essay API methods
export const EssayService = {
  // Get essay by ID
  getEssayById: async (essayId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ESSAYS.BY_ID(essayId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get essay by assessment ID
  getEssayByAssessment: async (assessmentId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ESSAYS.BY_ASSESSMENT(assessmentId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Module API methods
export const ModuleService = {
  // Get module with progress by ID
  getModuleWithProgress: async (moduleId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.MODULES.BY_ID(moduleId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get all modules in a lesson with progress
  getModulesWithProgress: async (lessonId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.MODULES.BY_LESSON(lessonId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


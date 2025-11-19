import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Teacher Package API methods
export const TeacherPackageService = {
  // Get list of teacher packages
  getTeacherPackages: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.TEACHER_PACKAGES.LIST);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get teacher package by ID
  getTeacherPackageById: async (id) => {
    try {
      return await httpClient.get(API_ENDPOINTS.TEACHER_PACKAGES.BY_ID(id));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


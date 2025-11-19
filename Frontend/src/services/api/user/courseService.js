import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Course API methods
export const CourseService = {
  // Get system courses (public, no auth required)
  getSystemCourses: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.COURSES.SYSTEM_COURSES);
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


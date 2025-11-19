import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Enrollment API methods
export const EnrollmentService = {
  // Enroll in a course
  enrollInCourse: async (enrollData) => {
    try {
      return await httpClient.post(API_ENDPOINTS.ENROLLMENT.ENROLL, enrollData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Unenroll from a course
  unenrollFromCourse: async (courseId) => {
    try {
      return await httpClient.delete(API_ENDPOINTS.ENROLLMENT.UNENROLL(courseId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get my enrolled courses
  getMyEnrolledCourses: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.ENROLLMENT.MY_COURSES);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Join course by class code
  joinByClassCode: async (classCode) => {
    try {
      return await httpClient.post(API_ENDPOINTS.ENROLLMENT.JOIN_BY_CLASS_CODE, { classCode });
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


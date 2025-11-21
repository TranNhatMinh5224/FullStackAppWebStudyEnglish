import { API_ENDPOINTS } from '../user/config.js';
import { httpClient } from '../user/httpClient.js';

// Admin API methods
export const AdminService = {
  // User Management
  getAllUsers: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.USERS.ALL);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  getTeachers: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.USERS.TEACHERS);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  getStudentsByAllCourses: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.USERS.STUDENTS_BY_COURSES);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  getBlockedAccounts: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.USERS.BLOCKED);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  blockAccount: async (userId) => {
    try {
      return await httpClient.put(API_ENDPOINTS.ADMIN.USERS.BLOCK(userId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  unblockAccount: async (userId) => {
    try {
      return await httpClient.put(API_ENDPOINTS.ADMIN.USERS.UNBLOCK(userId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Course Management
  getAllCourses: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.COURSES.ALL);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  createCourse: async (courseData) => {
    try {
      return await httpClient.post(API_ENDPOINTS.ADMIN.COURSES.CREATE, courseData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  updateCourse: async (courseId, courseData) => {
    try {
      return await httpClient.put(API_ENDPOINTS.ADMIN.COURSES.UPDATE(courseId), courseData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  deleteCourse: async (courseId) => {
    try {
      return await httpClient.delete(API_ENDPOINTS.ADMIN.COURSES.DELETE(courseId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  getUsersByCourse: async (courseId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.COURSES.USERS_BY_COURSE(courseId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Teacher Package Management
  getAllTeacherPackages: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.TEACHER_PACKAGES.ALL);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  getTeacherPackageById: async (id) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.TEACHER_PACKAGES.BY_ID(id));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  createTeacherPackage: async (packageData) => {
    try {
      return await httpClient.post(API_ENDPOINTS.ADMIN.TEACHER_PACKAGES.CREATE, packageData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  updateTeacherPackage: async (id, packageData) => {
    try {
      return await httpClient.put(API_ENDPOINTS.ADMIN.TEACHER_PACKAGES.UPDATE(id), packageData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  deleteTeacherPackage: async (id) => {
    try {
      return await httpClient.delete(API_ENDPOINTS.ADMIN.TEACHER_PACKAGES.DELETE(id));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Assessment Management
  createAssessment: async (assessmentData) => {
    try {
      return await httpClient.post(API_ENDPOINTS.ADMIN.ASSESSMENTS.CREATE, assessmentData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  getAssessmentsByModule: async (moduleId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.ASSESSMENTS.BY_MODULE(moduleId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  getAssessmentById: async (assessmentId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ADMIN.ASSESSMENTS.BY_ID(assessmentId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  updateAssessment: async (assessmentId, assessmentData) => {
    try {
      return await httpClient.put(API_ENDPOINTS.ADMIN.ASSESSMENTS.UPDATE(assessmentId), assessmentData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  deleteAssessment: async (assessmentId) => {
    try {
      return await httpClient.delete(API_ENDPOINTS.ADMIN.ASSESSMENTS.DELETE(assessmentId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


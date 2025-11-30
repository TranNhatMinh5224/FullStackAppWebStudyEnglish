import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

/**
 * Course API Service
 * Handles all course-related API calls
 */
export const CourseService = {
  /**
   * Get all system courses (public access)
   * Works for both authenticated and unauthenticated users
   * @returns {Promise} Course list with enrollment status if authenticated
   */
  getSystemCourses: async () => {
    try {
      const response = await httpClient.get(API_ENDPOINTS.COURSES.SYSTEM_COURSES);
      return response;
    } catch (error) {
      console.error('Error fetching system courses:', error);
      return { 
        success: false, 
        message: error.message || 'Failed to fetch courses',
        data: null 
      };
    }
  },

  /**
   * Get course by ID (detailed information)
   * @param {number} courseId - Course ID
   * @returns {Promise} Course details
   */
  getCourseById: async (courseId) => {
    try {
      const response = await httpClient.get(`${API_ENDPOINTS.COURSES.SYSTEM_COURSES}/${courseId}`);
      return response;
    } catch (error) {
      console.error(`Error fetching course ${courseId}:`, error);
      return { 
        success: false, 
        message: error.message || 'Failed to fetch course details',
        data: null 
      };
    }
  },

  /**
   * Search courses by keyword
   * @param {string} keyword - Search keyword
   * @returns {Promise} Filtered course list
   */
  searchCourses: async (keyword) => {
    try {
      const response = await httpClient.get(`${API_ENDPOINTS.COURSES.SYSTEM_COURSES}/search?keyword=${keyword}`);
      return response;
    } catch (error) {
      console.error('Error searching courses:', error);
      return { 
        success: false, 
        message: error.message || 'Failed to search courses',
        data: null 
      };
    }
  },

  /**
   * Get featured courses
   * @returns {Promise} Featured course list
   */
  getFeaturedCourses: async () => {
    try {
      const response = await httpClient.get(`${API_ENDPOINTS.COURSES.SYSTEM_COURSES}/featured`);
      return response;
    } catch (error) {
      console.error('Error fetching featured courses:', error);
      return { 
        success: false, 
        message: error.message || 'Failed to fetch featured courses',
        data: null 
      };
    }
  }
};


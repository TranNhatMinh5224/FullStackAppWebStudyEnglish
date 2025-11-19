import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Essay Submission API methods
export const EssaySubmissionService = {
  // Submit an essay
  submitEssay: async (submissionData) => {
    try {
      return await httpClient.post(API_ENDPOINTS.ESSAY_SUBMISSIONS.SUBMIT, submissionData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get essay submission by ID
  getSubmissionById: async (submissionId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ESSAY_SUBMISSIONS.BY_ID(submissionId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get my essay submissions
  getMySubmissions: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.ESSAY_SUBMISSIONS.MY_SUBMISSIONS);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get submission status by assessment ID
  getSubmissionStatus: async (assessmentId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.ESSAY_SUBMISSIONS.STATUS_BY_ASSESSMENT(assessmentId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Update essay submission
  updateSubmission: async (submissionId, updateData) => {
    try {
      return await httpClient.put(API_ENDPOINTS.ESSAY_SUBMISSIONS.UPDATE(submissionId), updateData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Delete essay submission
  deleteSubmission: async (submissionId) => {
    try {
      return await httpClient.delete(API_ENDPOINTS.ESSAY_SUBMISSIONS.DELETE(submissionId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Vocabulary Review API methods
export const VocabularyReviewService = {
  // Get due vocabulary words (words that need review)
  getDueWords: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.VOCABULARY_REVIEW.DUE);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get new vocabulary words
  getNewWords: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.VOCABULARY_REVIEW.NEW);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Start a vocabulary review session
  startReview: async (flashCardId) => {
    try {
      return await httpClient.post(API_ENDPOINTS.VOCABULARY_REVIEW.START(flashCardId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Submit vocabulary review result
  submitReview: async (reviewId, reviewData) => {
    try {
      return await httpClient.post(API_ENDPOINTS.VOCABULARY_REVIEW.SUBMIT(reviewId), reviewData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get vocabulary review statistics
  getStats: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.VOCABULARY_REVIEW.STATS);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get recently reviewed words
  getRecentReviews: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.VOCABULARY_REVIEW.RECENT);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Reset vocabulary review progress
  resetReview: async (flashCardId) => {
    try {
      return await httpClient.post(API_ENDPOINTS.VOCABULARY_REVIEW.RESET(flashCardId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


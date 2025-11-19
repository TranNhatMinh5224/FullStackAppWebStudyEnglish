import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// FlashCard API methods
export const FlashCardService = {
  // Get flashcard by ID
  getFlashCardById: async (id) => {
    try {
      return await httpClient.get(API_ENDPOINTS.FLASHCARDS.BY_ID(id));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get flashcards by module ID
  getFlashCardsByModule: async (moduleId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.FLASHCARDS.BY_MODULE(moduleId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Search flashcards
  searchFlashCards: async (searchTerm, moduleId = null) => {
    try {
      const params = new URLSearchParams({ searchTerm });
      if (moduleId) {
        params.append('moduleId', moduleId);
      }
      return await httpClient.get(`${API_ENDPOINTS.FLASHCARDS.SEARCH}?${params.toString()}`);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Get flashcard progress by module
  getFlashCardProgress: async (moduleId) => {
    try {
      return await httpClient.get(API_ENDPOINTS.FLASHCARDS.PROGRESS(moduleId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Reset flashcard progress
  resetFlashCardProgress: async (flashCardId) => {
    try {
      return await httpClient.post(API_ENDPOINTS.FLASHCARDS.RESET_PROGRESS(flashCardId));
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


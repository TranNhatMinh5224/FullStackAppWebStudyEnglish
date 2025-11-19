import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';

// Payment API methods
export const PaymentService = {
  // Process payment
  processPayment: async (paymentData) => {
    try {
      return await httpClient.post(API_ENDPOINTS.PAYMENTS.PROCESS, paymentData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Confirm payment
  confirmPayment: async (confirmationData) => {
    try {
      return await httpClient.post(API_ENDPOINTS.PAYMENTS.CONFIRM, confirmationData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};


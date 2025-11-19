import { API_ENDPOINTS } from './config.js';
import { httpClient } from './httpClient.js';
import { TokenManager } from './tokenManager.js';

// Authentication API methods
export const AuthService = {
  // Register user
  register: async (userData) => {
    try {
      const registerData = {
        firstName: userData.firstName,
        lastName: userData.lastName,
        email: userData.email,
        password: userData.password,
        phoneNumber: userData.phoneNumber
      };

      const result = await httpClient.post(API_ENDPOINTS.AUTH.REGISTER, registerData);
      return result;
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Login user
  login: async (credentials) => {
    try {
      console.log('[AuthService] Login request:', { email: credentials.email });
      const result = await httpClient.post(API_ENDPOINTS.AUTH.LOGIN, credentials);
      
      console.log('[AuthService] Login response:', {
        success: result.success,
        hasData: !!result.data,
        dataStructure: result.data ? Object.keys(result.data) : [],
        hasUser: !!result.data?.user,
        hasDataData: !!result.data?.data,
        user: result.data?.user || result.data?.data?.user,
        hasToken: !!(result.data?.accessToken || result.data?.data?.accessToken)
      });
      
      if (result.success && result.data) {
        // Backend response format: { success: true, message: "...", data: { accessToken, refreshToken, user, expiresAt } }
        // httpClient wraps it: { success: true, data: { success: true, message: "...", data: {...} } }
        // So we need to check both result.data.data and result.data
        const responseData = result.data.data || result.data;
        
        // Store tokens and user data
        if (responseData.accessToken && responseData.refreshToken) {
          TokenManager.setTokens(responseData.accessToken, responseData.refreshToken);
          console.log('[AuthService] Tokens stored');
        } else {
          console.error('[AuthService] Missing tokens in response!', responseData);
        }
        
        if (responseData.user) {
          localStorage.setItem('user', JSON.stringify(responseData.user));
          console.log('[AuthService] User stored:', responseData.user);
        } else {
          console.error('[AuthService] No user data in response!', responseData);
        }
        
        // Update result.data to point to the unwrapped data
        result.data = responseData;
      }

      return result;
    } catch (error) {
      console.error('[AuthService] Login error:', error);
      return { success: false, error: error.message };
    }
  },

  // Logout user
  logout: () => {
    TokenManager.clearTokens();
  },

  // Get user profile
  getProfile: async () => {
    try {
      return await httpClient.get(API_ENDPOINTS.AUTH.PROFILE);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Update user profile
  updateProfile: async (profileData) => {
    try {
      const updateData = {
        firstName: profileData.firstName,
        lastName: profileData.lastName,
        phoneNumber: profileData.phoneNumber
      };

      const result = await httpClient.put(API_ENDPOINTS.AUTH.UPDATE_PROFILE, updateData);
      
      if (result.success && result.data) {
        // Update stored user data
        localStorage.setItem('user', JSON.stringify(result.data));
      }

      return result;
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Change password
  changePassword: async (passwordData) => {
    try {
      return await httpClient.put(API_ENDPOINTS.AUTH.CHANGE_PASSWORD, passwordData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Forgot password
  forgotPassword: async (email) => {
    try {
      return await httpClient.post(API_ENDPOINTS.AUTH.FORGOT_PASSWORD, { email });
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Verify OTP
  verifyOTP: async (verifyData) => {
    try {
      // verifyData should contain: { email, otpCode }
      return await httpClient.post(API_ENDPOINTS.AUTH.VERIFY_OTP, verifyData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Reset password with OTP
  resetPassword: async (resetData) => {
    try {
      // resetData should contain: { email, otpCode, newPassword }
      return await httpClient.post(API_ENDPOINTS.AUTH.SET_NEW_PASSWORD, resetData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  },

  // Refresh token
  refreshToken: async (refreshToken) => {
    try {
      return await httpClient.post(API_ENDPOINTS.AUTH.REFRESH_TOKEN, { refreshToken });
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};
// API configuration and base setup
const BASE_URL = 'http://localhost:5029/api';

// API Endpoints
const API_ENDPOINTS = {
  AUTH: {
    REGISTER: 'user/auth/register',
    LOGIN: 'user/auth/login',
    PROFILE: 'user/auth/profile',
    UPDATE_PROFILE: 'user/auth/profile',
    CHANGE_PASSWORD: 'user/auth/change-password',
    FORGOT_PASSWORD: 'user/auth/forgot-password',
    RESET_PASSWORD: 'user/auth/reset-password'
  }
};

// Token management
export const TokenManager = {
  getAccessToken: () => localStorage.getItem('accessToken'),
  getRefreshToken: () => localStorage.getItem('refreshToken'),
  setTokens: (accessToken, refreshToken) => {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  },
  clearTokens: () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },
  isTokenExpired: (token) => {
    if (!token) return true;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 < Date.now();
    } catch {
      return true;
    }
  }
};

// HTTP Client with automatic token handling
export const httpClient = {
  async request(url, options = {}) {
    const token = TokenManager.getAccessToken();
    const isLoginRequest = url.includes('login');
    
    console.log('HTTP Request:', {
      url: `${BASE_URL}/${url}`,
      method: options.method || 'GET',
      hasToken: !!token,
      isLoginRequest,
      tokenPreview: token ? `${token.substring(0, 50)}...` : 'No token'
    });
    
    const config = {
      headers: {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...options.headers
      },
      ...options
    };

    if (config.body && typeof config.body === 'object') {
      config.body = JSON.stringify(config.body);
    }

    try {
      const response = await fetch(`${BASE_URL}/${url}`, config);
      
      console.log('HTTP Response:', {
        status: response.status,
        statusText: response.statusText,
        url: response.url
      });
      
      // Handle different response types
      const contentType = response.headers.get('content-type');
      let data;
      
      if (contentType && contentType.includes('application/json')) {
        data = await response.json();
      } else {
        data = await response.text();
      }

      if (!response.ok) {
        console.error('HTTP Error Response:', {
          status: response.status,
          data: data
        });
        
        // Handle 401 Unauthorized (but not for login requests)
        if (response.status === 401 && !isLoginRequest) {
          console.log('401 Unauthorized - clearing tokens');
          TokenManager.clearTokens();
          
          // Only redirect if not already on login page
          if (!window.location.pathname.includes('/login')) {
            console.log('Redirecting to login page');
            window.location.href = '/login';
          }
          
          throw new Error('Session expired. Please login again.');
        }
        
        // For login requests or other errors, just throw the error without redirect
        throw new Error(data.message || data || `HTTP error! status: ${response.status}`);
      }

      // Return success format
      return {
        success: true,
        data: data,
        status: response.status
      };

    } catch (error) {
      console.error('HTTP Request failed:', error);
      return {
        success: false,
        error: error.message
      };
    }
  },

  get(url, options = {}) {
    return this.request(url, { ...options, method: 'GET' });
  },

  post(url, data, options = {}) {
    return this.request(url, { ...options, method: 'POST', body: data });
  },

  put(url, data, options = {}) {
    return this.request(url, { ...options, method: 'PUT', body: data });
  },

  delete(url, options = {}) {
    return this.request(url, { ...options, method: 'DELETE' });
  }
};

// Authentication API methods
export const AuthAPI = {
  // Register user
  register: async (userData) => {
    try {
      const registerData = {
        sureName: userData.firstName,
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
      const result = await httpClient.post(API_ENDPOINTS.AUTH.LOGIN, credentials);
      
      if (result.success && result.data) {
        // Store tokens and user data
        TokenManager.setTokens(result.data.accessToken, result.data.refreshToken);
        localStorage.setItem('user', JSON.stringify(result.data.user));
      }

      return result;
    } catch (error) {
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
        sureName: profileData.firstName,
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

  // Reset password with OTP
  resetPassword: async (resetData) => {
    try {
      // resetData should contain: { email, otpCode, newPassword }
      return await httpClient.post(API_ENDPOINTS.AUTH.RESET_PASSWORD, resetData);
    } catch (error) {
      return { success: false, error: error.message };
    }
  }
};

export default AuthAPI;
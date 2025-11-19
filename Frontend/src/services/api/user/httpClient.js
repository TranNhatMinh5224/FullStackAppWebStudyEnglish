import { BASE_URL } from './config.js';
import { TokenManager } from './tokenManager.js';

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

      // Backend returns: { success: true, message: "...", data: {...} }
      // httpClient wraps it: { success: true, data: { success: true, message: "...", data: {...} }, status: 200 }
      // So we need to unwrap if data has success property
      let finalData = data;
      
      // If backend response is nested (has success and data properties), unwrap it
      if (data && typeof data === 'object' && 'success' in data && 'data' in data) {
        // Backend already returned { success, data, message }
        // We should return it as is, but check if we need to unwrap
        finalData = data;
      }
      
      console.log('[httpClient] Response data structure:', {
        hasSuccess: data && 'success' in data,
        hasData: data && 'data' in data,
        dataKeys: data ? Object.keys(data) : [],
        dataType: typeof data
      });
      
      // Return success format
      return {
        success: true,
        data: finalData,
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
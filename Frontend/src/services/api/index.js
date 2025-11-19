// Main API export
export * from './user/index.js';

// Re-export for easier imports
export { 
  AuthService as AuthAPI,
  TokenManager,
  httpClient,
  BASE_URL,
  API_ENDPOINTS 
} from './user/index.js';
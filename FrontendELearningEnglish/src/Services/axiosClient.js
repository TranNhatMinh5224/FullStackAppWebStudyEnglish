import axios from "axios";
import { tokenStorage } from "../Utils/tokenStorage";
import { API_BASE_URL, AUTH_REFRESH_URL } from "./BaseURL";

const axiosClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// ===== REQUEST =====
axiosClient.interceptors.request.use(
  (config) => {
    const token = tokenStorage.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ===== RESPONSE =====
let isRefreshing = false;
let queue = [];

const processQueue = (error, token = null) => {
  queue.forEach((p) => {
    if (error) p.reject(error);
    else p.resolve(token);
  });
  queue = [];
};

axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (
      error.response?.status === 401 &&
      !originalRequest._retry
    ) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          queue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return axiosClient(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = tokenStorage.getRefreshToken();
        const expiredAccessToken = tokenStorage.getAccessToken();
        if (!refreshToken || !expiredAccessToken) throw error;

        // Call refresh API with both refreshToken and current (expired) accessToken
        const res = await axios.post(
          AUTH_REFRESH_URL,
          { refreshToken, accessToken: expiredAccessToken }
        );

        // Backend wraps data in ServiceResponse<T>
        const { accessToken, refreshToken: newRefresh } = res?.data?.data || {};
        if (!accessToken || !newRefresh) throw error;
        tokenStorage.setTokens({ accessToken, refreshToken: newRefresh });

        axiosClient.defaults.headers.Authorization = `Bearer ${accessToken}`;
        processQueue(null, accessToken);

        return axiosClient(originalRequest);
      } catch (err) {
        processQueue(err, null);
        const hadTokens = tokenStorage.getAccessToken() || tokenStorage.getRefreshToken();
        tokenStorage.clear();
        
        // Only redirect to login if we actually had tokens (not a guest user)
        // Guest users should be allowed to stay on the page
        if (hadTokens) {
          // Only redirect if we're not already on a public page
          const currentPath = window.location.pathname;
          const publicPaths = ['/welcome', '/login', '/register', '/home'];
          if (!publicPaths.includes(currentPath)) {
            window.location.href = "/login";
          }
        }
        return Promise.reject(err);
      } finally {
        isRefreshing = false;
      }
    }

    // For 401 errors that don't trigger token refresh (e.g., guest users)
    // Don't redirect if user is on a public page or home page
    if (error.response?.status === 401) {
      const currentPath = window.location.pathname;
      const allowedPaths = ['/welcome', '/login', '/register', '/home'];
      const hasToken = tokenStorage.getAccessToken();
      
      // If no token and on allowed path, don't redirect (guest user)
      if (!hasToken && allowedPaths.includes(currentPath)) {
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);

export default axiosClient;

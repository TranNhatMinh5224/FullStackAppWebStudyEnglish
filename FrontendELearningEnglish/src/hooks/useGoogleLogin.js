import { useState, useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../Context/AuthContext";
import { ROUTE_PATHS } from "../Routes/Paths";

/**
 * Custom hook for Google Login
 * Handles all Google OAuth login logic
 * Builds OAuth URL from environment variables
 */
export const useGoogleLogin = () => {
  const navigate = useNavigate();
  const { googleLogin } = useAuth();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleGoogleLogin = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      // Check if Google Client ID is configured
      const googleClientId = process.env.REACT_APP_GOOGLE_CLIENT_ID;
      if (!googleClientId) {
        throw new Error("Google Client ID chưa được cấu hình. Vui lòng kiểm tra file .env");
      }

      // Generate CSRF state token (backend requirement)
      const state =
        Math.random().toString(36).substring(2, 15) +
        Math.random().toString(36).substring(2, 15);

      // Store state in sessionStorage for verification after redirect
      sessionStorage.setItem("google_oauth_state", state);

      // Build redirect URI
      const frontendUrl = process.env.REACT_APP_FRONTEND_URL || window.location.origin;
      const redirectUri = `${frontendUrl}${ROUTE_PATHS.GOOGLE_CALLBACK}`;

      // Build Google OAuth URL
      const googleAuthUrl = new URL("https://accounts.google.com/o/oauth2/v2/auth");
      googleAuthUrl.searchParams.set("client_id", googleClientId);
      googleAuthUrl.searchParams.set("redirect_uri", redirectUri);
      googleAuthUrl.searchParams.set("response_type", "code");
      googleAuthUrl.searchParams.set("scope", "openid email profile");
      googleAuthUrl.searchParams.set("state", state);
      googleAuthUrl.searchParams.set("access_type", "offline");
      googleAuthUrl.searchParams.set("prompt", "consent");

      // Redirect to Google OAuth consent screen
      window.location.href = googleAuthUrl.toString();
    } catch (err) {
      console.error("Google login error:", err);
      const errorMessage =
        err.message ||
        "Đăng nhập bằng Google thất bại. Vui lòng thử lại.";
      setError(errorMessage);
      setLoading(false);
    }
  }, []);

  return {
    handleGoogleLogin,
    loading,
    error,
  };
};


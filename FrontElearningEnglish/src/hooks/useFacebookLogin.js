import { useState, useCallback } from "react";
import { ROUTE_PATHS } from "../Routes/Paths";

/**
 * Custom hook for Facebook Login
 * Handles all Facebook OAuth login logic
 * Builds OAuth URL from environment variables
 */
export const useFacebookLogin = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleFacebookLogin = useCallback(async () => {
    setLoading(true);
    setError("");

    try {
      // Check if Facebook App ID is configured
      const facebookAppId = process.env.REACT_APP_FACEBOOK_APP_ID;
      if (!facebookAppId) {
        throw new Error("Facebook App ID chưa được cấu hình. Vui lòng kiểm tra file .env");
      }

      // Generate CSRF state token (backend requirement)
      const state =
        Math.random().toString(36).substring(2, 15) +
        Math.random().toString(36).substring(2, 15);

      // Store state in sessionStorage for verification after redirect
      sessionStorage.setItem("facebook_oauth_state", state);

      // Build redirect URI
      const frontendUrl = process.env.REACT_APP_FRONTEND_URL || window.location.origin;
      const redirectUri = `${frontendUrl}${ROUTE_PATHS.FACEBOOK_CALLBACK}`;

      // Build Facebook OAuth URL
      const facebookAuthUrl = new URL("https://www.facebook.com/v18.0/dialog/oauth");
      facebookAuthUrl.searchParams.set("client_id", facebookAppId);
      facebookAuthUrl.searchParams.set("redirect_uri", redirectUri);
      facebookAuthUrl.searchParams.set("response_type", "code");
      facebookAuthUrl.searchParams.set("scope", "email,public_profile");
      facebookAuthUrl.searchParams.set("state", state);

      // Redirect to Facebook OAuth consent screen
      window.location.href = facebookAuthUrl.toString();
    } catch (err) {
      console.error("Facebook login error:", err);
      const errorMessage =
        err.message ||
        "Đăng nhập bằng Facebook thất bại. Vui lòng thử lại.";
      setError(errorMessage);
      setLoading(false);
    }
  }, []);

  return {
    handleFacebookLogin,
    loading,
    error,
  };
};


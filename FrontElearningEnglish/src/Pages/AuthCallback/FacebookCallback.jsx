import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../../Context/AuthContext";
import "./AuthCallback.css";

export default function FacebookCallback() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { facebookLogin } = useAuth();
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const handleCallback = async () => {
      try {
        // Get authorization code and state from URL
        const code = searchParams.get("code");
        const state = searchParams.get("state");
        const errorParam = searchParams.get("error");
        const errorReason = searchParams.get("error_reason");
        const errorDescription = searchParams.get("error_description");

        // Check for OAuth errors
        if (errorParam) {
          let errorMsg = "Đăng nhập bằng Facebook đã bị hủy.";
          if (errorReason === "user_denied") {
            errorMsg = "Bạn đã từ chối quyền truy cập Facebook.";
          } else if (errorDescription) {
            errorMsg = errorDescription;
          }
          setError(errorMsg);
          setLoading(false);
          setTimeout(() => navigate("/login"), 3000);
          return;
        }

        // Validate code and state
        if (!code || !state) {
          setError("Thiếu thông tin xác thực. Vui lòng thử lại.");
          setLoading(false);
          setTimeout(() => navigate("/login"), 3000);
          return;
        }

        // Verify state token (CSRF protection)
        const storedState = sessionStorage.getItem("facebook_oauth_state");
        sessionStorage.removeItem("facebook_oauth_state");

        if (storedState !== state) {
          setError("Lỗi bảo mật. Vui lòng thử lại.");
          setLoading(false);
          setTimeout(() => navigate("/login"), 3000);
          return;
        }

        // Send code and state to backend
        await facebookLogin(
          {
            Code: code,
            State: state,
          },
          navigate
        );
      } catch (err) {
        console.error("Facebook callback error:", err);
        const errorMessage =
          err.response?.data?.message ||
          err.message ||
          "Đăng nhập bằng Facebook thất bại.";
        setError(errorMessage);
        setLoading(false);
        setTimeout(() => navigate("/login"), 3000);
      }
    };

    handleCallback();
  }, [searchParams, navigate, facebookLogin]);

  return (
    <div className="auth-callback-container">
      <div className="auth-callback-card">
        {loading ? (
          <>
            <div className="auth-callback-spinner"></div>
            <p>Đang xử lý đăng nhập Facebook...</p>
          </>
        ) : (
          <>
            <div className="auth-callback-error">⚠️</div>
            <p>{error || "Có lỗi xảy ra"}</p>
            <p className="auth-callback-redirect">
              Đang chuyển hướng về trang đăng nhập...
            </p>
          </>
        )}
      </div>
    </div>
  );
}


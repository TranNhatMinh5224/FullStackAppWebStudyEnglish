import React, { useState, useEffect } from "react";
import "./Login.css";
import Header from "../../Components/Header/LogoHeader";
import { useNavigate } from "react-router-dom";
import { FcGoogle } from "react-icons/fc";
import { FaFacebookF, FaUser } from "react-icons/fa";
import { useAuth } from "../../Context/AuthContext";
import { InputField, SocialLoginButton } from "../../Components/Auth";
import { useGoogleLogin, useFacebookLogin } from "../../hooks";

export default function Login() {
  const navigate = useNavigate();
  const { login, loginAsGuest } = useAuth();

  // Use custom hooks for social login
  const {
    handleGoogleLogin,
    loading: googleLoading,
    error: googleError,
  } = useGoogleLogin();
  const {
    handleFacebookLogin,
    loading: facebookLoading,
    error: facebookError,
  } = useFacebookLogin();

  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });

  const [showPassword, setShowPassword] = useState(false);
  const [errors, setErrors] = useState({
    email: "",
    password: "",
  });
  const [generalError, setGeneralError] = useState("");
  const [loading, setLoading] = useState(false);
  const [socialLoading, setSocialLoading] = useState({
    google: false,
    facebook: false,
    guest: false,
  });

  // Update social loading state from hooks
  useEffect(() => {
    setSocialLoading((prev) => ({
      ...prev,
      google: googleLoading,
      facebook: facebookLoading,
    }));
  }, [googleLoading, facebookLoading]);

  // Update general error from hooks
  useEffect(() => {
    if (googleError) {
      setGeneralError(googleError);
    } else if (facebookError) {
      setGeneralError(facebookError);
    }
  }, [googleError, facebookError]);


  // Validate email format
  const validateEmail = (email) => {
    if (!email) {
      return "Vui lòng nhập email";
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      return "Email không hợp lệ, email phải có định dạng example@example.com";
    }
    return "";
  };

  // Validate password
  const validatePassword = (password) => {
    if (!password) {
      return "Vui lòng nhập mật khẩu";
    }
    if (password.length < 6) {
      return "Mật khẩu phải có ít nhất 6 ký tự";
    }
    return "";
  };

  // Handle input change with validation
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));

    // Real-time validation
    setGeneralError("");
    if (name === "email") {
      setErrors((prev) => ({
        ...prev,
        email: validateEmail(value),
      }));
    } else if (name === "password") {
      setErrors((prev) => ({
        ...prev,
        password: validatePassword(value),
      }));
    }
  };

  // Handle form submission
  const handleLogin = async (e) => {
    e.preventDefault();
    setGeneralError("");

    // Validate all fields
    const emailError = validateEmail(formData.email);
    const passwordError = validatePassword(formData.password);

    setErrors({
      email: emailError,
      password: passwordError,
    });

    if (emailError || passwordError) {
      return;
    }

    setLoading(true);
    try {
      // login returns a promise, so we can use .then() or await
      await login({ email: formData.email, password: formData.password }, navigate);
    } catch (err) {
      setGeneralError(
        err.response?.data?.message ||
        err.message ||
        "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin."
      );
    } finally {
      setLoading(false);
    }
  };


  // Handle Guest Login
  const handleGuestLogin = () => {
    setSocialLoading((prev) => ({ ...prev, guest: true }));
    loginAsGuest(navigate);
  };

  return (
    <div className="auth-container">
      <Header />

      <div className="auth-card">
        <h1 className="auth-title">Chào mừng trở lại!</h1>
        <p className="auth-subtitle">Đăng nhập để tiếp tục hành trình của bạn.</p>

        {/* General error message */}
        {generalError && (
          <div className="auth-error-message">{generalError}</div>
        )}

        <form onSubmit={handleLogin}>
          {/* Email Input */}
          <InputField
            type="email"
            name="email"
            placeholder="email@gmail.com"
            value={formData.email}
            onChange={handleInputChange}
            error={errors.email}
            disabled={loading}
          />

          {/* Password Input */}
          <InputField
            type="password"
            name="password"
            placeholder="Nhập mật khẩu của bạn"
            value={formData.password}
            onChange={handleInputChange}
            error={errors.password}
            disabled={loading}
            showPasswordToggle={true}
            showPassword={showPassword}
            onTogglePassword={() => setShowPassword(!showPassword)}
          />

          {/* Options */}
          <div className="auth-options">
            <label>
              <input type="checkbox" /> Remember me
            </label>
            <span
              className="auth-link"
              onClick={() => navigate("/forgot-password")}
              style={{ cursor: "pointer" }}
            >
              Quên mật khẩu?
            </span>
          </div>

          {/* Login button */}
          <button
            className="auth-btn primary"
            type="submit"
            disabled={loading}
          >
            {loading ? "Đang đăng nhập..." : "Đăng nhập"}
          </button>
        </form>

        {/* Register */}
        <p className="auth-footer">
          Chưa có tài khoản?{" "}
          <span className="auth-link" onClick={() => navigate("/register")}>
            Đăng ký
          </span>
        </p>

        <div className="divider">HOẶC</div>

        {/* Social login buttons */}
        <SocialLoginButton
          type="google"
          icon={FcGoogle}
          text="Đăng nhập bằng Google"
          onClick={handleGoogleLogin}
          disabled={loading}
          loading={socialLoading.google}
        />

        <SocialLoginButton
          type="facebook"
          icon={FaFacebookF}
          text="Đăng nhập bằng Facebook"
          onClick={handleFacebookLogin}
          disabled={loading}
          loading={socialLoading.facebook}
        />

        <SocialLoginButton
          type="guest"
          icon={FaUser}
          text="Đăng nhập bằng khách"
          onClick={handleGuestLogin}
          disabled={loading}
          loading={socialLoading.guest}
        />
      </div>
    </div>
  );
}

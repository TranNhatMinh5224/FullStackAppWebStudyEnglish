import React, { useState } from "react";
import "./LoginScreen.css";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { images } from "../assets/images";
import { Cloud } from "../components";

const LoginScreen = () => {
  const [formData, setFormData] = useState({
    email: "",
    password: ""
  });
  const [isLoading, setIsLoading] = useState(false);
  const [fieldErrors, setFieldErrors] = useState({});
  const [localError, setLocalError] = useState("");
  
  const navigate = useNavigate();
  const { login, error, clearError } = useAuth();

  const validateField = (name, value) => {
    const errors = { ...fieldErrors };

    switch (name) {
      case 'email':
        if (!value.trim()) {
          errors.email = "Email không được để trống";
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
          errors.email = "Email không hợp lệ";
        } else {
          delete errors.email;
        }
        break;
      case 'password':
        if (!value.trim()) {
          errors.password = "Mật khẩu không được để trống";
        } else if (value.length < 6) {
          errors.password = "Mật khẩu phải có ít nhất 6 ký tự";
        } else {
          delete errors.password;
        }
        break;
      default:
        break;
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    
    // Clear all errors when user starts typing
    if (error) clearError();
    if (localError) setLocalError("");
    
    setFormData({
      ...formData,
      [name]: value
    });

    // Validate field on change (after user has interacted with it)
    if (fieldErrors[name] || value.trim() === '') {
      validateField(name, value);
    }
  };

  const handleBlur = (e) => {
    const { name, value } = e.target;
    validateField(name, value);
  };

  const validateForm = () => {
    const emailValid = validateField('email', formData.email);
    const passwordValid = validateField('password', formData.password);
    
    if (!emailValid || !passwordValid) {
      setLocalError("Vui lòng điền đầy đủ thông tin hợp lệ");
      return false;
    }
    
    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    e.stopPropagation(); // Prevent any event bubbling
    setLocalError("");
    
    console.log('[DEBUG] Login submit started');
    
    // Validate form before submission
    if (!validateForm()) {
      console.log('[DEBUG] Form validation failed');
      return;
    }
    
    setIsLoading(true);
    console.log('[DEBUG] Calling login API with:', { email: formData.email, password: '***' });
    
    try {
      // Login with API
      const loginResult = await login(formData);
      
      console.log('[DEBUG] Login result:', loginResult);
      
      if (loginResult && loginResult.success) {
        console.log('[DEBUG] Login successful, navigating to home');
        navigate("/home");
      } else {
        console.log('[DEBUG] Login failed, showing error. Result:', loginResult);
        // Handle specific login errors
        const errorMsg = loginResult?.error || 'Unknown error';
        if (errorMsg.includes('Invalid email or password')) {
          setLocalError("Mật khẩu không đúng");
        } else {
          setLocalError(errorMsg || "Đăng nhập thất bại. Vui lòng thử lại!");
        }
        console.log('[DEBUG] Error message set:', localError);
      }
    } catch (err) {
      console.error('[DEBUG] Login exception:', err);
      setLocalError("Có lỗi xảy ra. Vui lòng thử lại!");
    } finally {
      console.log('[DEBUG] Setting loading to false');
      setIsLoading(false);
    }
  };

  return (
    <div className="login-container">
      {/* Clouds decorations */}
      <Cloud src={images.cloud1} position="top-left" />
      <Cloud src={images.cloud2} position="top-right" />
      <Cloud src={images.cloud3} position="bottom-right" />
      
      <button 
        className="back-button" 
        onClick={() => navigate("/")}
        aria-label="Quay lại"
      />
      <div className="login-form">
        <h2>Đăng nhập</h2>
        
        {(error || localError) && (
          <div className="error-message">
            {localError || error}
          </div>
        )}
        
        <form onSubmit={handleSubmit}>
          <div className="input-group">
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Email"
              disabled={isLoading}
              className={fieldErrors.email ? 'error' : ''}
            />
            {fieldErrors.email && (
              <span className="field-error">{fieldErrors.email}</span>
            )}
          </div>
          
          <div className="input-group">
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              onBlur={handleBlur}
              placeholder="Mật khẩu"
              disabled={isLoading}
              className={fieldErrors.password ? 'error' : ''}
            />
            {fieldErrors.password && (
              <span className="field-error">{fieldErrors.password}</span>
            )}
          </div>
          
          <button 
            type="submit" 
            disabled={isLoading || Object.keys(fieldErrors).length > 0}
            className={isLoading ? 'loading' : ''}
          >
            {isLoading ? 'Đang đăng nhập...' : 'Đăng nhập'}
          </button>
        </form>
        
        <div className="forgot-password-link">
          <Link to="/forgot-password">Quên mật khẩu?</Link>
        </div>
        
        <p>
          Chưa có tài khoản? <Link to="/register">Đăng ký ngay</Link>
        </p>
      </div>
    </div>
  );
};

export default LoginScreen;



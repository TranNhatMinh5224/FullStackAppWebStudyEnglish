import React, { useState } from "react";
import "./ForgotPasswordScreen.css";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { images } from "../../assets/images";
import { Cloud } from "../../components";

const ForgotPasswordScreen = () => {
  const [email, setEmail] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [localError, setLocalError] = useState("");
  
  const navigate = useNavigate();
  const { sendPasswordResetEmail, error, clearError } = useAuth();

  const handleChange = (e) => {
    if (error) clearError();
    if (localError) setLocalError("");
    setEmail(e.target.value);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setLocalError("");
    
    try {
      // Validate email format
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(email)) {
        setLocalError("Vui lòng nhập địa chỉ email hợp lệ!");
        setIsLoading(false);
        return;
      }

      // Send OTP to email
      const result = await sendPasswordResetEmail(email);
      
      if (result.success) {
        // Navigate to OTP verification screen
        navigate("/otp-verification", { 
          state: { email: email } 
        });
      } else {
        setLocalError(result.error || "Có lỗi xảy ra. Vui lòng thử lại!");
      }
    } catch (err) {
      console.error('Forgot password error:', err);
      setLocalError("Có lỗi xảy ra. Vui lòng thử lại!");
    } finally {
      setIsLoading(false);
    }
  };



  return (
    <div className="forgot-password-container">
      {/* Clouds decorations */}
      <Cloud src={images.cloud1} position="top-left" />
      <Cloud src={images.cloud2} position="top-right" />
      <Cloud src={images.cloud3} position="bottom-right" />
      
      <button 
        className="back-button" 
        onClick={() => navigate("/login")}
        aria-label="Quay lại"
      />

      <div className="forgot-password-form">
        <div className="form-header">
          <div className="icon-container">
            <div className="forgot-icon">🔒</div>
          </div>
          <h2>Quên mật khẩu?</h2>
          <p className="form-description">
            Đừng lo lắng! Hãy nhập email của bạn và chúng tôi sẽ gửi 
            mã OTP để đặt lại mật khẩu cho bạn.
          </p>
        </div>
        
        {(error || localError) && (
          <div className="error-message">
            {error || localError}
          </div>
        )}
            
        <form onSubmit={handleSubmit}>
          <div className="input-group">
            <label htmlFor="email">Địa chỉ email</label>
            <input
              type="email"
              id="email"
              name="email"
              value={email}
              onChange={handleChange}
              placeholder="Nhập email của bạn"
              required
              disabled={isLoading}
            />
          </div>
          
          <button 
            type="submit" 
            disabled={isLoading || !email.trim()}
            className={`submit-btn ${isLoading ? 'loading' : ''}`}
          >
            {isLoading ? (
              <>
                <span className="loading-spinner"></span>
                Đang gửi...
              </>
            ) : (
              'Gửi mã OTP'
            )}
          </button>
        </form>

        <div className="form-footer">
          <p>
            Nhớ mật khẩu rồi? <Link to="/login">Đăng nhập ngay</Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default ForgotPasswordScreen;
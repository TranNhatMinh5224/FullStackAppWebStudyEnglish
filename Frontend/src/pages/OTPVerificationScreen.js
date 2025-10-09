import React, { useState, useRef, useEffect } from "react";
import "./OTPVerificationScreen.css";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { images } from "../assets/images";
import { Cloud } from "../components";

const OTPVerificationScreen = () => {
  const [otpCode, setOtpCode] = useState(["", "", "", "", "", ""]);
  const [isLoading, setIsLoading] = useState(false);
  const [localError, setLocalError] = useState("");
  const [timer, setTimer] = useState(900); // 15 minutes in seconds
  
  const navigate = useNavigate();
  const location = useLocation();
  const { verifyOTP, resendOTP, error, clearError } = useAuth();
  const inputRefs = useRef([]);
  
  const email = location.state?.email;

  // Redirect if no email provided
  useEffect(() => {
    if (!email) {
      navigate("/forgot-password");
    }
  }, [email, navigate]);

  // Timer countdown
  useEffect(() => {
    if (timer > 0) {
      const interval = setInterval(() => {
        setTimer(prev => prev - 1);
      }, 1000);
      return () => clearInterval(interval);
    }
  }, [timer]);

  const formatTime = (seconds) => {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  };

  const handleOtpChange = (index, value) => {
    if (error) clearError();
    if (localError) setLocalError("");

    // Only allow digits
    if (!/^\d*$/.test(value)) return;

    const newOtp = [...otpCode];
    newOtp[index] = value;
    setOtpCode(newOtp);

    // Auto focus next input
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (index, e) => {
    // Handle backspace
    if (e.key === 'Backspace' && !otpCode[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handlePaste = (e) => {
    e.preventDefault();
    const pastedData = e.clipboardData.getData('text');
    
    if (!/^\d{6}$/.test(pastedData)) {
      setLocalError("Vui lòng dán mã OTP 6 số hợp lệ!");
      return;
    }

    const newOtp = pastedData.split('');
    setOtpCode(newOtp);
    inputRefs.current[5]?.focus();
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const fullOtp = otpCode.join('');
    
    if (fullOtp.length !== 6) {
      setLocalError("Vui lòng nhập đầy đủ 6 số OTP!");
      return;
    }

    if (timer <= 0) {
      setLocalError("Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới!");
      return;
    }

    setIsLoading(true);
    setLocalError("");
    
    try {
      const result = await verifyOTP(email, fullOtp);
      
      if (result.success) {
        // Navigate to reset password screen with verified token
        navigate("/reset-password", { 
          state: { 
            email: email,
            otpCode: fullOtp,
            verified: true 
          } 
        });
      }
    } catch (err) {
      console.error('OTP verification error:', err);
      setLocalError("Mã OTP không chính xác hoặc đã hết hạn!");
    } finally {
      setIsLoading(false);
    }
  };

  const handleResendOtp = async () => {
    setIsLoading(true);
    setLocalError("");
    
    try {
      const result = await resendOTP(email);
      if (result.success) {
        setOtpCode(["", "", "", "", "", ""]);
        setTimer(900); // Reset timer to 15 minutes
        inputRefs.current[0]?.focus();
        console.log('New OTP sent to:', email);
      }
    } catch (err) {
      console.error('Resend OTP error:', err);
      setLocalError("Có lỗi xảy ra khi gửi lại mã OTP!");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="otp-verification-container">
      {/* Clouds decorations */}
      <Cloud src={images.cloud1} position="top-left" />
      <Cloud src={images.cloud2} position="top-right" />
      <Cloud src={images.cloud3} position="bottom-right" />
      
      <button 
        className="back-button" 
        onClick={() => navigate("/forgot-password")}
        aria-label="Quay lại"
      />

      <div className="otp-verification-form">
        <div className="form-header">
          <div className="icon-container">
            <div className="otp-icon">🔐</div>
          </div>
          <h2>Xác thực OTP</h2>
          <p className="form-description">
            Chúng tôi đã gửi mã OTP 6 số đến email:
          </p>
          <div className="email-display">{email}</div>
          <p className="instruction-text">
            Vui lòng nhập mã OTP để tiếp tục đặt lại mật khẩu.
          </p>
        </div>
        
        {(error || localError) && (
          <div className="error-message">
            {error || localError}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="otp-input-group">
            <label>Mã OTP (6 số)</label>
            <div className="otp-inputs">
              {otpCode.map((digit, index) => (
                <input
                  key={index}
                  ref={ref => inputRefs.current[index] = ref}
                  type="text"
                  maxLength="1"
                  value={digit}
                  onChange={(e) => handleOtpChange(index, e.target.value)}
                  onKeyDown={(e) => handleKeyDown(index, e)}
                  onPaste={index === 0 ? handlePaste : undefined}
                  disabled={isLoading}
                  className="otp-input"
                  autoComplete="off"
                />
              ))}
            </div>
          </div>

          <div className="timer-container">
            <p className={`timer ${timer <= 60 ? 'warning' : ''}`}>
              ⏱️ Mã OTP sẽ hết hạn sau: <strong>{formatTime(timer)}</strong>
            </p>
          </div>
          
          <button 
            type="submit" 
            disabled={isLoading || otpCode.join('').length !== 6 || timer <= 0}
            className={`submit-btn ${isLoading ? 'loading' : ''}`}
          >
            {isLoading ? (
              <>
                <span className="loading-spinner"></span>
                Đang xác thực...
              </>
            ) : (
              'Xác thực OTP'
            )}
          </button>
        </form>

        <div className="action-buttons">
          <button 
            className="resend-btn"
            onClick={handleResendOtp}
            disabled={isLoading || timer > 840} // Disable for first 60 seconds
          >
            {isLoading ? 'Đang gửi lại...' : 'Gửi lại mã OTP'}
          </button>
          
          <button 
            className="back-to-login-btn"
            onClick={() => navigate("/login")}
          >
            Quay lại đăng nhập
          </button>
        </div>

        <div className="form-footer">
          <p>
            Không nhận được mã? Kiểm tra thư mục spam hoặc{" "}
            <span 
              className="resend-link" 
              onClick={handleResendOtp}
              style={{ 
                cursor: timer <= 840 ? 'pointer' : 'not-allowed',
                opacity: timer <= 840 ? 1 : 0.5 
              }}
            >
              gửi lại
            </span>
          </p>
          <p>
            Nhớ mật khẩu rồi? <Link to="/login">Đăng nhập ngay</Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default OTPVerificationScreen;
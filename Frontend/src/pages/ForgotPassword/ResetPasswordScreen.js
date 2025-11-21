import React, { useState, useEffect } from "react";
import "./ResetPasswordScreen.css";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { images } from "../../assets/images";
import { Cloud } from "../../components";

const ResetPasswordScreen = () => {
  const location = useLocation();
  const [formData, setFormData] = useState({
    newPassword: "",
    confirmPassword: ""
  });
  const [isLoading, setIsLoading] = useState(false);
  const [isTokenValid, setIsTokenValid] = useState(true);
  const [isPasswordReset, setIsPasswordReset] = useState(false);
  const [localError, setLocalError] = useState("");
  const [passwordStrength, setPasswordStrength] = useState({
    score: 0,
    feedback: ""
  });
  
  const navigate = useNavigate();
  const { resetPassword, error, clearError } = useAuth();
  
  // Get data from navigation state (from OTP verification)
  const email = location.state?.email;
  const otpCode = location.state?.otpCode;
  const verified = location.state?.verified;

  useEffect(() => {
    // Check if user came from OTP verification
    if (!email || !otpCode || !verified) {
      setIsTokenValid(false);
      // Redirect back to forgot password if not properly verified
      navigate("/forgot-password");
    }
  }, [email, otpCode, verified, navigate]);

  const calculatePasswordStrength = (password) => {
    let score = 0;
    let feedback = "";

    if (password.length >= 8) score += 1;
    if (/[a-z]/.test(password)) score += 1;
    if (/[A-Z]/.test(password)) score += 1;
    if (/[0-9]/.test(password)) score += 1;
    if (/[^A-Za-z0-9]/.test(password)) score += 1;

    switch (score) {
      case 0:
      case 1:
        feedback = "Rất yếu";
        break;
      case 2:
        feedback = "Yếu";
        break;
      case 3:
        feedback = "Trung bình";
        break;
      case 4:
        feedback = "Mạnh";
        break;
      case 5:
        feedback = "Rất mạnh";
        break;
      default:
        feedback = "";
    }

    return { score, feedback };
  };

  const handleChange = (e) => {
    if (error) clearError();
    if (localError) setLocalError("");
    
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value
    });

    // Calculate password strength for new password
    if (name === 'newPassword') {
      setPasswordStrength(calculatePasswordStrength(value));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setLocalError("");
    
    try {
      // Validate passwords
      if (formData.newPassword !== formData.confirmPassword) {
        setLocalError("Mật khẩu xác nhận không khớp!");
        setIsLoading(false);
        return;
      }

      if (formData.newPassword.length < 6) {
        setLocalError("Mật khẩu phải có ít nhất 6 ký tự!");
        setIsLoading(false);
        return;
      }

      if (passwordStrength.score < 2) {
        setLocalError("Mật khẩu quá yếu! Vui lòng chọn mật khẩu mạnh hơn.");
        setIsLoading(false);
        return;
      }

      // Reset password with OTP
      const result = await resetPassword({
        email,
        otpCode,
        newPassword: formData.newPassword,
        confirmPassword: formData.confirmPassword
      });
      
      if (result.success) {
        setIsPasswordReset(true);
        // Redirect to login after 3 seconds
        setTimeout(() => {
          navigate("/login");
        }, 3000);
      } else {
        // Show error if reset failed
        setLocalError(result.error || "Có lỗi xảy ra khi đặt lại mật khẩu. Vui lòng thử lại!");
      }
    } catch (err) {
      console.error('Reset password error:', err);
      setLocalError(err.message || "Có lỗi xảy ra. Vui lòng thử lại!");
    } finally {
      setIsLoading(false);
    }
  };

  // Invalid access or not verified
  if (!isTokenValid) {
    return (
      <div className="reset-password-container">
        <Cloud src={images.cloud1} position="top-left" />
        <Cloud src={images.cloud2} position="top-right" />
        <Cloud src={images.cloud3} position="bottom-right" />
        
        <div className="reset-password-form error-state">
          <div className="form-header">
            <div className="icon-container">
              <div className="error-icon">❌</div>
            </div>
            <h2>Truy cập không hợp lệ</h2>
            <p className="form-description">
              Bạn cần xác thực OTP trước khi đặt lại mật khẩu. 
              Vui lòng quay lại và thực hiện đúng quy trình.
            </p>
          </div>

          <div className="action-buttons">
            <Link to="/forgot-password" className="primary-btn">
              Quên mật khẩu
            </Link>
            <Link to="/login" className="secondary-btn">
              Quay lại đăng nhập
            </Link>
          </div>
        </div>
      </div>
    );
  }

  // Password reset successful
  if (isPasswordReset) {
    return (
      <div className="reset-password-container">
        <Cloud src={images.cloud1} position="top-left" />
        <Cloud src={images.cloud2} position="top-right" />
        <Cloud src={images.cloud3} position="bottom-right" />
        
        <div className="reset-password-form success-state">
          <div className="form-header">
            <div className="icon-container">
              <div className="success-icon">✅</div>
            </div>
            <h2>Đặt lại mật khẩu thành công!</h2>
            <p className="form-description">
              Mật khẩu của bạn đã được cập nhật thành công. 
              Bạn sẽ được chuyển đến trang đăng nhập sau 3 giây.
            </p>
          </div>

          <div className="action-buttons">
            <Link to="/login" className="primary-btn">
              Đăng nhập ngay
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="reset-password-container">
      {/* Clouds decorations */}
      <Cloud src={images.cloud1} position="top-left" />
      <Cloud src={images.cloud2} position="top-right" />
      <Cloud src={images.cloud3} position="bottom-right" />
      
      <button 
        className="back-button" 
        onClick={() => navigate("/login")}
        aria-label="Quay lại"
      />

      <div className="reset-password-form">
        <div className="form-header">
          <div className="icon-container">
            <div className="reset-icon">🔑</div>
          </div>
          <h2>Đặt lại mật khẩu</h2>
          <p className="form-description">
            Tạo mật khẩu mới cho tài khoản: <strong>{email}</strong>
          </p>
        </div>
        
        {(error || localError) && (
          <div className="error-message">
            {error || localError}
          </div>
        )}
        
        <form onSubmit={handleSubmit}>
          <div className="input-group">
            <label htmlFor="newPassword">Mật khẩu mới</label>
            <input
              type="password"
              id="newPassword"
              name="newPassword"
              value={formData.newPassword}
              onChange={handleChange}
              placeholder="Nhập mật khẩu mới"
              required
              disabled={isLoading}
            />
            {formData.newPassword && (
              <div className={`password-strength strength-${passwordStrength.score}`}>
                <div className="strength-bar">
                  <div 
                    className="strength-fill" 
                    style={{ width: `${(passwordStrength.score / 5) * 100}%` }}
                  ></div>
                </div>
                <span className="strength-text">
                  Độ mạnh: {passwordStrength.feedback}
                </span>
              </div>
            )}
          </div>
          
          <div className="input-group">
            <label htmlFor="confirmPassword">Xác nhận mật khẩu</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="Nhập lại mật khẩu mới"
              required
              disabled={isLoading}
            />
            {formData.confirmPassword && (
              <div className={`password-match ${
                formData.newPassword === formData.confirmPassword ? 'match' : 'no-match'
              }`}>
                {formData.newPassword === formData.confirmPassword ? '✓ Mật khẩu khớp' : '✗ Mật khẩu không khớp'}
              </div>
            )}
          </div>

          <div className="password-requirements">
            <h4>Yêu cầu mật khẩu:</h4>
            <ul>
              <li className={formData.newPassword.length >= 8 ? 'valid' : ''}>
                Ít nhất 8 ký tự
              </li>
              <li className={/[a-z]/.test(formData.newPassword) ? 'valid' : ''}>
                Có chữ thường
              </li>
              <li className={/[A-Z]/.test(formData.newPassword) ? 'valid' : ''}>
                Có chữ hoa
              </li>
              <li className={/[0-9]/.test(formData.newPassword) ? 'valid' : ''}>
                Có số
              </li>
            </ul>
          </div>
          
          <button 
            type="submit" 
            disabled={isLoading || !formData.newPassword || !formData.confirmPassword}
            className={`submit-btn ${isLoading ? 'loading' : ''}`}
          >
            {isLoading ? (
              <>
                <span className="loading-spinner"></span>
                Đang cập nhật...
              </>
            ) : (
              'Cập nhật mật khẩu'
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

export default ResetPasswordScreen;
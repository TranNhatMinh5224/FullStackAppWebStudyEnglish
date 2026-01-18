import React, { useState } from "react";
import { Container, Row, Col, Form, Button, Alert } from "react-bootstrap";
import { FaLock } from "react-icons/fa";
import "./ForgotPassword.css";
import { authService } from "../../Services/authService";
import { useNavigate } from "react-router-dom";
import { InputField } from "../../Components/Auth";

export default function ForgotPassword() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);
  const [emailError, setEmailError] = useState("");

  // Validate email
  const validateEmail = (emailValue) => {
    if (!emailValue) {
      return "Vui lòng nhập email";
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(emailValue)) {
      return "Email không hợp lệ, email phải có định dạng example@example.com";
    }
    return "";
  };

  // Handle input change with validation
  const handleEmailChange = (e) => {
    const value = e.target.value;
    setEmail(value);
    setEmailError(validateEmail(value));
    setError("");
    setSuccess("");
  };

  const handleSendOTP = async (e) => {
    e?.preventDefault();
    setError("");
    setSuccess("");

    // Validate email
    const emailValidationError = validateEmail(email);
    if (emailValidationError) {
      setEmailError(emailValidationError);
      return;
    }

    setLoading(true);
    try {
      const res = await authService.forgotPassword({ email: email.trim() });

      if (res.data && res.data.success === true) {
        setSuccess("OTP đã được gửi đến email của bạn!");
        setEmailError("");

        setTimeout(() => {
          navigate("/reset-otp", { state: { email: email.trim() } });
        }, 800);
      } else {
        setError(res.data?.message || "Không thể gửi mã OTP. Vui lòng thử lại.");
      }
    } catch (err) {
      const msg = err.response?.data?.message || "Email không tồn tại hoặc không hợp lệ.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="forgot-password-container">
      <div className="forgot-password-background">
        <div className="forgot-password-shape shape-cyan"></div>
        <div className="forgot-password-shape shape-pink"></div>
      </div>

      <Container>
        <Row className="justify-content-center">
          <Col xs={12} sm={10} md={8} lg={6} xl={5}>
            <div className="forgot-password-card">
              <div className="forgot-password-icon-wrapper">
                <FaLock className="forgot-password-icon" />
              </div>

              <h1 className="forgot-password-title">Quên mật khẩu?</h1>

              <p className="forgot-password-description">
                Nhập email bạn đã dùng để đăng ký, chúng tôi sẽ gửi mã OTP để đặt lại mật khẩu.
              </p>

              <Form onSubmit={handleSendOTP}>
                <Form.Group className="mb-3">
                  <Form.Label className="forgot-password-label">Email</Form.Label>
                  <InputField
                    type="email"
                    name="email"
                    placeholder="email@example.com"
                    value={email}
                    onChange={handleEmailChange}
                    error={emailError}
                    disabled={loading}
                  />
                </Form.Group>

                {error && <Alert variant="danger" className="forgot-password-error-message">{error}</Alert>}
                {success && <Alert variant="success" className="forgot-password-success-message">{success}</Alert>}

                <div className="d-grid mb-3">
                  <Button
                    variant="primary"
                    type="submit"
                    onClick={handleSendOTP}
                    disabled={loading || !!emailError}
                    className="forgot-password-submit-btn"
                    size="lg"
                  >
                    {loading ? "Đang gửi..." : "Gửi mã OTP"}
                  </Button>
                </div>
              </Form>

              <div className="d-grid">
                <Button
                  variant="link"
                  onClick={() => navigate("/login")}
                  disabled={loading}
                  className="forgot-password-back-link"
                >
                  Quay lại Đăng nhập
                </Button>
              </div>
            </div>
          </Col>
        </Row>
      </Container>
    </div>
  );
}

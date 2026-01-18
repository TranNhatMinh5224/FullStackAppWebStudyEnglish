import React, { useState, useEffect } from "react";
import { Container, Row, Col, Form, Button, Alert } from "react-bootstrap";
import { FaLock } from "react-icons/fa";
import "./ResetPassword.css";
import { authService } from "../../Services/authService";
import { useNavigate, useLocation } from "react-router-dom";
import { InputField } from "../../Components/Auth";
import SuccessModal from "../../Components/Common/SuccessModal/SuccessModal";

export default function ResetPassword() {
  const navigate = useNavigate();
  const { state } = useLocation();
  const email = state?.email;
  const otpCode = state?.otpCode;

  useEffect(() => {
    if (!email || !otpCode) {
      navigate("/forgot-password");
    }
  }, [email, otpCode, navigate]);

  const [formData, setFormData] = useState({
    newPassword: "",
    confirmPassword: "",
  });

  const [errors, setErrors] = useState({
    newPassword: "",
    confirmPassword: "",
  });

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  // Validate password requirements (giống như ChangePassword nhưng đơn giản hơn)
  const validatePassword = (password) => {
    const errors = [];
    if (password.length < 6) {
      errors.push("ít nhất 6 ký tự");
    }
    if (password.length > 20) {
      errors.push("không quá 20 ký tự");
    }
    if (!/[A-Z]/.test(password)) {
      errors.push("chữ hoa");
    }
    if (!/[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?]/.test(password)) {
      errors.push("ký tự đặc biệt");
    }
    return errors;
  };

  const validateField = (name, value) => {
    const newErrors = { ...errors };

    switch (name) {
      case "newPassword":
        if (!value) {
          newErrors.newPassword = "Vui lòng nhập mật khẩu mới";
        } else {
          const passwordErrors = validatePassword(value);
          if (passwordErrors.length > 0) {
            newErrors.newPassword = `Mật khẩu phải có ${passwordErrors.join(", ")}`;
          } else {
            newErrors.newPassword = "";
          }

          // Validate confirm password again if it has value
          if (formData.confirmPassword) {
            if (value !== formData.confirmPassword) {
              newErrors.confirmPassword = "Mật khẩu không khớp";
            } else {
              newErrors.confirmPassword = "";
            }
          }
        }
        break;

      case "confirmPassword":
        if (!value) {
          newErrors.confirmPassword = "Vui lòng nhập lại mật khẩu";
        } else if (formData.newPassword && value !== formData.newPassword) {
          newErrors.confirmPassword = "Mật khẩu không khớp";
        } else {
          newErrors.confirmPassword = "";
        }
        break;

      default:
        break;
    }

    setErrors(newErrors);
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
    setError("");
    setSuccess("");
    validateField(name, value);
  };

  const validateForm = () => {
    const newErrors = { ...errors };

    // Validate new password
    if (!formData.newPassword) {
      newErrors.newPassword = "Vui lòng nhập mật khẩu mới";
    } else {
      const passwordErrors = validatePassword(formData.newPassword);
      if (passwordErrors.length > 0) {
        newErrors.newPassword = `Mật khẩu phải có ${passwordErrors.join(", ")}`;
      } else {
        newErrors.newPassword = "";
      }
    }

    // Validate confirm password
    if (!formData.confirmPassword) {
      newErrors.confirmPassword = "Vui lòng nhập lại mật khẩu";
    } else if (formData.newPassword !== formData.confirmPassword) {
      newErrors.confirmPassword = "Mật khẩu không khớp";
    } else {
      newErrors.confirmPassword = "";
    }

    setErrors(newErrors);

    // Check if form is valid
    return !newErrors.newPassword && !newErrors.confirmPassword;
  };

  const handleReset = async (e) => {
    e?.preventDefault();
    setError("");
    setSuccess("");

    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      const res = await authService.resetPassword({
        email,
        otpCode,
        newPassword: formData.newPassword,
        confirmPassword: formData.confirmPassword,
      });

      if (res.data?.success) {
        const msg = "Đặt lại mật khẩu thành công! Bạn sẽ được chuyển tới trang đăng nhập.";
        setSuccess(msg);
        setSuccessMessage(msg);
        setShowSuccessModal(true);
      } else {
        setError(res.data?.message || "Đặt lại mật khẩu thất bại. Vui lòng thử lại.");
      }
    } catch (err) {
      setError(err.response?.data?.message || "Đặt lại mật khẩu thất bại. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  };

  if (!email || !otpCode) {
    return null;
  }

  return (
    <>
      <div className="reset-password-container">
        <div className="reset-password-background">
          <div className="reset-password-shape shape-cyan"></div>
          <div className="reset-password-shape shape-pink"></div>
        </div>

        <Container>
          <Row className="justify-content-center">
            <Col xs={12} sm={10} md={8} lg={6} xl={5}>
              <div className="reset-password-card">
                <div className="reset-password-icon-wrapper">
                  <FaLock className="reset-password-icon" />
                </div>

                <h1 className="reset-password-title">Tạo mật khẩu mới</h1>

                <p className="reset-password-description">
                  Đặt lại mật khẩu cho email <strong>{email}</strong>
                </p>

                <Form onSubmit={handleReset}>
                  <Form.Group className="mb-3">
                    <Form.Label className="reset-password-label">Mật khẩu mới</Form.Label>
                    <InputField
                      type="password"
                      name="newPassword"
                      placeholder="Nhập mật khẩu mới"
                      value={formData.newPassword}
                      onChange={handleInputChange}
                      error={errors.newPassword}
                      disabled={loading}
                      showPasswordToggle={true}
                      showPassword={showPassword}
                      onTogglePassword={() => setShowPassword(!showPassword)}
                      maxLength={20}
                    />
                  </Form.Group>

                  <Form.Group className="mb-3">
                    <Form.Label className="reset-password-label">Xác nhận mật khẩu</Form.Label>
                    <InputField
                      type="password"
                      name="confirmPassword"
                      placeholder="Nhập lại mật khẩu mới"
                      value={formData.confirmPassword}
                      onChange={handleInputChange}
                      error={errors.confirmPassword}
                      disabled={loading}
                      showPasswordToggle={true}
                      showPassword={showConfirmPassword}
                      onTogglePassword={() => setShowConfirmPassword(!showConfirmPassword)}
                      maxLength={20}
                    />
                  </Form.Group>

                  {error && <Alert variant="danger" className="reset-password-alert-error">{error}</Alert>}
                  {success && <Alert variant="success" className="reset-password-alert-success">{success}</Alert>}

                  <div className="d-grid">
                    <Button
                      type="submit"
                      variant="primary"
                      size="lg"
                      className="reset-password-submit-btn"
                      disabled={loading || !!errors.newPassword || !!errors.confirmPassword}
                    >
                      {loading ? "Đang xử lý..." : "Đặt lại mật khẩu"}
                    </Button>
                  </div>
                </Form>

                <div className="text-center mt-3">
                  <Button
                    variant="link"
                    className="reset-password-back-link"
                    onClick={() => navigate("/login")}
                    disabled={loading}
                  >
                    Quay lại Đăng nhập
                  </Button>
                </div>
              </div>
            </Col>
          </Row>
        </Container>
      </div>

      <SuccessModal
        isOpen={showSuccessModal}
        onClose={() => {
          setShowSuccessModal(false);
          navigate("/login");
        }}
        title="Thành công"
        message={successMessage || "Đặt lại mật khẩu thành công."}
        autoClose={true}
        autoCloseDelay={1500}
      />
    </>
  );
}

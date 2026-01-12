import React, { useState, useEffect } from "react";
import { Modal, Form, Button, Alert } from "react-bootstrap";
import { superAdminService } from "../../../Services/superAdminService";
import ConfirmModal from "../../Common/ConfirmModal/ConfirmModal";
import "./CreateAdminModal.css";

// Validation patterns (theo backend)
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_REGEX = /^[0-9]{10,11}$/;
const MIN_PASSWORD_LENGTH = 6;

export default function CreateAdminModal({ show, onClose, onSuccess }) {
  const [formData, setFormData] = useState({
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    phoneNumber: "",
    roleId: 2, // Default: ContentAdmin
  });
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});
  const [showConfirmClose, setShowConfirmClose] = useState(false);

  // Reset form when modal closes
  useEffect(() => {
    if (!show) {
      setFormData({
        email: "",
        password: "",
        firstName: "",
        lastName: "",
        phoneNumber: "",
        roleId: 2,
      });
      setErrors({});
      setShowConfirmClose(false);
    }
  }, [show]);

  // Check if form has data
  const hasFormData = () => {
    return (
      formData.email.trim() !== "" ||
      formData.password !== "" ||
      formData.firstName.trim() !== "" ||
      formData.lastName.trim() !== "" ||
      formData.phoneNumber.trim() !== ""
    );
  };

  // Handle close with confirmation
  const handleClose = () => {
    if (hasFormData() && !loading) {
      setShowConfirmClose(true);
    } else {
      onClose();
    }
  };

  // Handle confirm close
  const handleConfirmClose = () => {
    setShowConfirmClose(false);
    onClose();
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: name === "roleId" ? parseInt(value) : value }));
    // Clear error for this field when user types
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: null }));
    }
  };

  // Validation function (theo backend)
  const validateForm = () => {
    const newErrors = {};

    // Email validation
    if (!formData.email.trim()) {
      newErrors.email = "Email là bắt buộc";
    } else if (!EMAIL_REGEX.test(formData.email)) {
      newErrors.email = "Email không hợp lệ";
    }

    // Password validation
    if (!formData.password) {
      newErrors.password = "Mật khẩu là bắt buộc";
    } else if (formData.password.length < MIN_PASSWORD_LENGTH) {
      newErrors.password = `Mật khẩu phải có ít nhất ${MIN_PASSWORD_LENGTH} ký tự`;
    }

    // FirstName validation
    if (!formData.firstName.trim()) {
      newErrors.firstName = "Họ là bắt buộc";
    } else if (formData.firstName.trim().length > 50) {
      newErrors.firstName = "Họ không được quá 50 ký tự";
    }

    // LastName validation
    if (!formData.lastName.trim()) {
      newErrors.lastName = "Tên là bắt buộc";
    } else if (formData.lastName.trim().length > 50) {
      newErrors.lastName = "Tên không được quá 50 ký tự";
    }

    // Phone validation (optional but must be valid if provided)
    if (formData.phoneNumber.trim() && !PHONE_REGEX.test(formData.phoneNumber.trim())) {
      newErrors.phoneNumber = "Số điện thoại phải có 10-11 chữ số";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      setLoading(true);
      const submitData = {
        email: formData.email.trim(),
        password: formData.password,
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        phoneNumber: formData.phoneNumber.trim() || null,
        roleId: formData.roleId,
      };

      const response = await superAdminService.createAdmin(submitData);
      
      if (response.data?.success) {
        onSuccess(response.data?.message || "Tạo admin thành công!");
      } else {
        setErrors({ submit: response.data?.message || "Không thể tạo admin" });
      }
    } catch (err) {
      console.error("Error creating admin:", err);
      const errorMessage = err.response?.data?.message || "Lỗi kết nối server";
      setErrors({ submit: errorMessage });
    } finally {
      setLoading(false);
    }
  };

  const isFormValid = formData.email && formData.password && formData.firstName && formData.lastName;

  return (
    <>
      <Modal 
        show={show} 
        onHide={handleClose} 
        centered
        className="modal-modern create-admin-modal"
        dialogClassName="create-admin-modal-dialog"
        backdrop="static"
        keyboard={false}
      >
        <Modal.Header closeButton>
          <Modal.Title>Tạo Admin Mới</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {errors.submit && <Alert variant="danger">{errors.submit}</Alert>}
          <Form onSubmit={handleSubmit}>
            <div className="form-section">
              <div className="section-title">Thông tin đăng nhập</div>
              
              <Form.Group className="mb-3">
                <Form.Label>Email <span className="text-danger">*</span></Form.Label>
                <Form.Control
                  type="email"
                  name="email"
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="admin@example.com"
                  isInvalid={!!errors.email}
                />
                <Form.Control.Feedback type="invalid">{errors.email}</Form.Control.Feedback>
              </Form.Group>

              <Form.Group className="mb-3">
                <Form.Label>Mật khẩu <span className="text-danger">*</span></Form.Label>
                <Form.Control
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  placeholder="Tối thiểu 6 ký tự"
                  isInvalid={!!errors.password}
                />
                <Form.Control.Feedback type="invalid">{errors.password}</Form.Control.Feedback>
              </Form.Group>
            </div>

            <div className="form-section">
              <div className="section-title">Thông tin cá nhân</div>
              
              <div className="row">
                <div className="col-md-6">
                  <Form.Group className="mb-3">
                    <Form.Label>Họ <span className="text-danger">*</span></Form.Label>
                    <Form.Control
                      type="text"
                      name="firstName"
                      value={formData.firstName}
                      onChange={handleChange}
                      placeholder="Nguyễn"
                      isInvalid={!!errors.firstName}
                    />
                    <Form.Control.Feedback type="invalid">{errors.firstName}</Form.Control.Feedback>
                  </Form.Group>
                </div>
                <div className="col-md-6">
                  <Form.Group className="mb-3">
                    <Form.Label>Tên <span className="text-danger">*</span></Form.Label>
                    <Form.Control
                      type="text"
                      name="lastName"
                      value={formData.lastName}
                      onChange={handleChange}
                      placeholder="Văn A"
                      isInvalid={!!errors.lastName}
                    />
                    <Form.Control.Feedback type="invalid">{errors.lastName}</Form.Control.Feedback>
                  </Form.Group>
                </div>
              </div>

              <Form.Group className="mb-3">
                <Form.Label>Số điện thoại</Form.Label>
                <Form.Control
                  type="tel"
                  name="phoneNumber"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  placeholder="0123456789"
                  isInvalid={!!errors.phoneNumber}
                />
                <Form.Control.Feedback type="invalid">{errors.phoneNumber}</Form.Control.Feedback>
              </Form.Group>
            </div>

            <div className="form-section">
              <div className="section-title">Phân quyền</div>
              
              <Form.Group className="mb-3">
                <Form.Label>Vai trò <span className="text-danger">*</span></Form.Label>
                <Form.Select
                  name="roleId"
                  value={formData.roleId}
                  onChange={handleChange}
                >
                  <option value={2}>ContentAdmin</option>
                  <option value={3}>FinanceAdmin</option>
                </Form.Select>
                <Form.Text className="text-muted">
                  Chọn vai trò cho admin mới
                </Form.Text>
              </Form.Group>
            </div>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleClose} disabled={loading}>
            Hủy
          </Button>
          <Button variant="primary" onClick={handleSubmit} disabled={!isFormValid || loading}>
            {loading ? "Đang tạo..." : "Tạo Admin"}
          </Button>
        </Modal.Footer>
      </Modal>

      {/* Confirm Close Modal */}
      <ConfirmModal
        isOpen={showConfirmClose}
        onClose={() => setShowConfirmClose(false)}
        onConfirm={handleConfirmClose}
        title="Xác nhận đóng"
        message="Bạn có dữ liệu chưa được lưu. Bạn có chắc chắn muốn hủy tạo admin không?"
        confirmText="Đóng"
        cancelText="Tiếp tục"
        type="warning"
      />
    </>
  );
}

import React, { useState, useRef } from "react";
import "./UpdateProfileScreen.css";
import { useAuth } from "../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import { images } from "../assets/images";
import { Cloud } from "../components";

const UpdateProfileScreen = () => {
  const { user, updateProfile, error, clearError } = useAuth();
  const navigate = useNavigate();
  const fileInputRef = useRef(null);

  const [formData, setFormData] = useState({
    firstName: user?.firstName || "",
    lastName: user?.lastName || "",
    phoneNumber: user?.phoneNumber || "",
    currentPassword: "",
    newPassword: "",
    confirmNewPassword: ""
  });

  const [avatar, setAvatar] = useState(user?.avatar || images.logo2);
  const [avatarFile, setAvatarFile] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [localError, setLocalError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const handleChange = (e) => {
    if (error) clearError();
    if (localError) setLocalError("");
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  const handleAvatarChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith('image/')) {
        setLocalError("Vui lòng chọn file hình ảnh!");
        return;
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        setLocalError("Kích thước file không được vượt quá 5MB!");
        return;
      }

      setAvatarFile(file);
      
      // Preview image
      const reader = new FileReader();
      reader.onload = (e) => {
        setAvatar(e.target.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setLocalError("");
    setSuccessMessage("");

    try {
      // Validate password if changing
      if (formData.newPassword || formData.confirmNewPassword) {
        if (!formData.currentPassword) {
          setLocalError("Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu!");
          setIsLoading(false);
          return;
        }

        if (formData.newPassword !== formData.confirmNewPassword) {
          setLocalError("Mật khẩu mới không khớp!");
          setIsLoading(false);
          return;
        }

        if (formData.newPassword.length < 6) {
          setLocalError("Mật khẩu mới phải có ít nhất 6 ký tự!");
          setIsLoading(false);
          return;
        }
      }

      // Validate phone number
      const phoneRegex = /^(0[3|5|7|8|9])+([0-9]{8})$/;
      if (formData.phoneNumber && !phoneRegex.test(formData.phoneNumber)) {
        setLocalError("Số điện thoại không hợp lệ!");
        setIsLoading(false);
        return;
      }

      // Prepare update data
      const updateData = {
        firstName: formData.firstName,
        lastName: formData.lastName,
        name: `${formData.firstName} ${formData.lastName}`,
        email: user?.email, // Keep existing email
        phoneNumber: formData.phoneNumber,
        avatar: avatar
      };

      // Add password if changing
      if (formData.newPassword) {
        updateData.currentPassword = formData.currentPassword;
        updateData.password = formData.newPassword;
      }

      // Simulate avatar upload if new file selected
      if (avatarFile) {
        // In real app, you would upload to server here
        updateData.avatar = avatar;
      }

      const result = await updateProfile(updateData);

      if (result.success) {
        setSuccessMessage("Cập nhật thông tin thành công!");
        // Clear password fields
        setFormData({
          ...formData,
          currentPassword: "",
          newPassword: "",
          confirmNewPassword: ""
        });
        
        // Redirect after success
        setTimeout(() => {
          navigate("/home");
        }, 2000);
      }
    } catch (err) {
      console.error('Update profile error:', err);
      setLocalError("Có lỗi xảy ra khi cập nhật thông tin!");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="update-profile-container">
      {/* Clouds decorations */}
      <Cloud src={images.cloud1} position="top-left" />
      <Cloud src={images.cloud2} position="top-right" />
      <Cloud src={images.cloud3} position="bottom-right" />
      
      <button 
        className="back-button" 
        onClick={() => navigate("/home")}
        aria-label="Quay lại"
      />

      <div className="update-profile-form">
        <h2>Cập nhật thông tin tài khoản</h2>
        
        {(error || localError) && (
          <div className="error-message">
            {error || localError}
          </div>
        )}

        {successMessage && (
          <div className="success-message">
            {successMessage}
          </div>
        )}
        
        <form onSubmit={handleSubmit}>
          {/* Avatar Section */}
          <div className="avatar-section">
            <div className="avatar-container" onClick={handleAvatarClick}>
              <img src={avatar} alt="Avatar" className="profile-avatar" />
              <div className="avatar-overlay">
                <span>Đổi ảnh</span>
              </div>
            </div>
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleAvatarChange}
              accept="image/*"
              style={{ display: 'none' }}
            />
          </div>

          {/* Personal Information */}
          <div className="form-section">
            <h3>Thông tin cá nhân</h3>
            <div className="form-row">
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                placeholder="Họ"
                required
                disabled={isLoading}
              />
              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                placeholder="Tên"
                required
                disabled={isLoading}
              />
            </div>
            <input
              type="tel"
              name="phoneNumber"
              value={formData.phoneNumber}
              onChange={handleChange}
              placeholder="Số điện thoại"
              disabled={isLoading}
            />
          </div>

          {/* Password Section */}
          <div className="form-section">
            <h3>Đổi mật khẩu (tùy chọn)</h3>
            <input
              type="password"
              name="currentPassword"
              value={formData.currentPassword}
              onChange={handleChange}
              placeholder="Mật khẩu hiện tại"
              disabled={isLoading}
            />
            <input
              type="password"
              name="newPassword"
              value={formData.newPassword}
              onChange={handleChange}
              placeholder="Mật khẩu mới"
              disabled={isLoading}
            />
            <input
              type="password"
              name="confirmNewPassword"
              value={formData.confirmNewPassword}
              onChange={handleChange}
              placeholder="Xác nhận mật khẩu mới"
              disabled={isLoading}
            />
          </div>

          <div className="form-actions">
            <button 
              type="button"
              className="btn-cancel"
              onClick={() => navigate("/home")}
              disabled={isLoading}
            >
              Hủy
            </button>
            <button 
              type="submit"
              className="btn-save"
              disabled={isLoading}
            >
              {isLoading ? 'Đang cập nhật...' : 'Lưu thay đổi'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default UpdateProfileScreen;
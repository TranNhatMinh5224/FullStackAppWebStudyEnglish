import React, { useState } from "react";
import "./RegisterScreen.css";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { images } from "../assets/images";
import { Cloud } from "../components";

const RegisterScreen = () => {
  const [formData, setFormData] = useState({
    fullName: "",
    email: "",
    password: "",
    confirmPassword: ""
  });
  const [isLoading, setIsLoading] = useState(false);
  const [localError, setLocalError] = useState("");
  
  const navigate = useNavigate();
  const { register, error, clearError } = useAuth();

  const handleChange = (e) => {
    if (error) clearError(); // Clear auth context error
    if (localError) setLocalError(""); // Clear local error
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    
    if (formData.password !== formData.confirmPassword) {
      setLocalError("Mật khẩu không khớp!");
      setIsLoading(false);
      return;
    }
    
    try {
      // Register user
      const result = register({
        name: formData.fullName,
        email: formData.email,
        password: formData.password
      });
      
      if (result.success) {
        alert("Đăng ký thành công! Vui lòng đăng nhập.");
        navigate("/login");
      }
    } catch (err) {
      console.error('Registration error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="register-container">
      {/* Clouds decorations */}
      <Cloud src={images.cloud1} position="top-left" />
      <Cloud src={images.cloud2} position="top-right" />
      <Cloud src={images.cloud3} position="bottom-right" />
      
      <button 
        className="back-button" 
        onClick={() => navigate("/")}
        aria-label="Quay lại"
      />
      <div className="register-form">
        <h2>Đăng ký tài khoản</h2>
        
        {(error || localError) && (
          <div className="error-message">
            {error || localError}
          </div>
        )}
        
        <form onSubmit={handleSubmit}>
          <input
            type="text"
            name="fullName"
            value={formData.fullName}
            onChange={handleChange}
            placeholder="Họ và tên"
            required
            disabled={isLoading}
          />
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            placeholder="Email"
            required
            disabled={isLoading}
          />
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            placeholder="Mật khẩu"
            required
            disabled={isLoading}
          />
          <input
            type="password"
            name="confirmPassword"
            value={formData.confirmPassword}
            onChange={handleChange}
            placeholder="Xác nhận mật khẩu"
            required
            disabled={isLoading}
          />
          <button 
            type="submit"
            disabled={isLoading}
            className={isLoading ? 'loading' : ''}
          >
            {isLoading ? 'Đang đăng ký...' : 'Đăng ký'}
          </button>
        </form>
        <p>
          Đã có tài khoản? <Link to="/login">Đăng nhập ngay</Link>
        </p>
      </div>
    </div>
  );
};

export default RegisterScreen;



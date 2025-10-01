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
  
  const navigate = useNavigate();
  const { login, validateLogin, error, clearError } = useAuth();

  const handleChange = (e) => {
    if (error) clearError(); // Clear error when user starts typing
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    
    try {
      // Validate login credentials
      const validation = validateLogin(formData.email, formData.password);
      
      if (validation.success) {
        // Proceed with login
        const loginResult = login(validation.user);
        
        if (loginResult.success) {
          navigate("/home");
        }
      }
    } catch (err) {
      console.error('Login error:', err);
    } finally {
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
        
        {error && (
          <div className="error-message">
            {error}
          </div>
        )}
        
        <form onSubmit={handleSubmit}>
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
          <button 
            type="submit" 
            disabled={isLoading}
            className={isLoading ? 'loading' : ''}
          >
            {isLoading ? 'Đang đăng nhập...' : 'Đăng nhập'}
          </button>
        </form>
        <p>
          Chưa có tài khoản? <Link to="/register">Đăng ký ngay</Link>
        </p>
      </div>
    </div>
  );
};

export default LoginScreen;



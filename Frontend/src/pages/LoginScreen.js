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
  
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    
    // Check if user exists (from registration)
    const registeredUser = localStorage.getItem('registeredUser');
    
    if (registeredUser) {
      const user = JSON.parse(registeredUser);
      
      // Simple validation
      if (formData.email === user.email && formData.password === user.password) {
        login(user);
        navigate("/home");
      } else {
        alert("Email hoặc mật khẩu không đúng!");
      }
    } else {
      alert("Tài khoản không tồn tại. Vui lòng đăng ký trước!");
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
        <form onSubmit={handleSubmit}>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            placeholder="Email"
            required
          />
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            placeholder="Mật khẩu"
            required
          />
          <button type="submit">Đăng nhập</button>
        </form>
        <p>
          Chưa có tài khoản? <Link to="/register">Đăng ký ngay</Link>
        </p>
      </div>
    </div>
  );
};

export default LoginScreen;



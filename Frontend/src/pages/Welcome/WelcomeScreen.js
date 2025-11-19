import React from "react";
import "./WelcomeScreen.css";
import { images } from "../../assets/images";
import { Cloud } from "../../components";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";

const WelcomeScreen = () => {
  const navigate = useNavigate();
  const { enterAsGuest } = useAuth();

  const handleStartClick = () => {
    enterAsGuest();
    navigate("/home");
  };

  return (
    <div className="welcome-container">
      {/* Clouds decorations */}
      <Cloud src={images.cloud1} position="top-left" />
      <Cloud src={images.cloud2} position="top-right" />
      <Cloud src={images.cloud3} position="bottom-right" />
      
      {/* Logo và title */}
      <img src={images.logo} alt="Logo" className="welcome-logo" />
      <span className="welcome-title">Catalunya English</span>

      {/* Main content với layout trái-phải */}
      <div className="main-content">
        {/* Mascot bên trái */}
        <div className="left-side">
          <img src={images.mascot} alt="Mascot" className="mascot" />
        </div>

        {/* Text và buttons bên phải */}
        <div className="right-side">
          <p className="slogan">Ghi nhớ 1000 từ vựng trong 1 tháng</p>
          
          <div className="button-group">
            <button className="custom-btn start" onClick={handleStartClick}>Bắt đầu</button>
            <button className="custom-btn login" onClick={() => navigate("/login")}>Đăng nhập</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default WelcomeScreen;



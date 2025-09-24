import React, { useEffect } from "react";
import "./IntroScreen.css";
import { images } from "../assets/images";
import { Cloud } from "../components";

const IntroScreen = ({ onIntroComplete }) => {
  useEffect(() => {
    const timer = setTimeout(() => {
      if (onIntroComplete) {
        onIntroComplete();
      }
    }, 3000); // 3 seconds

    return () => clearTimeout(timer);
  }, [onIntroComplete]);

  return (
    <div className="intro-container">
      {/* Clouds decorations */}
      <Cloud src={images.cloud1} position="top-left" />
      <Cloud src={images.cloud2} position="top-right" />
      <Cloud src={images.cloud3} position="bottom-right" />
      
      {/* Logo và title */}
      <img src={images.logo} alt="Logo" className="intro-logo" />
      <span className="intro-title">Catalunya English</span>

      <div className="mascot-section">
        <img src={images.mascot} alt="Mascot" className="mascot" />
        <h2>Chào mừng bạn đến với Catalunya English</h2>
      </div>

      <div className="loading-section">
        <div className="loading-dots">
          <div className="dot"></div>
          <div className="dot"></div>
          <div className="dot"></div>
        </div>
        <p className="loading-text">Đang tải...</p>
      </div>
    </div>
  );
};

export default IntroScreen;
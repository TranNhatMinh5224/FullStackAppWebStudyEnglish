import React from "react";
import "./WelcomeHero.css";
import { welcome as welcomeImage, mochiWelcome, mochiLoading } from "../../Assets";
import { useAuth } from "../../Context/AuthContext";
import { useNavigate } from "react-router-dom";

export default function WelcomeHero() {
  const navigate = useNavigate();
  const { loginAsGuest } = useAuth();

  const handleStartNow = () => {
    try {
      console.log("WelcomeHero: Bắt đầu đăng nhập bằng tài khoản khách");
      if (typeof loginAsGuest === 'function') {
        loginAsGuest(navigate);
      } else {
        console.error("loginAsGuest is not a function:", typeof loginAsGuest);
        // Fallback: manually set guest state and navigate
        navigate("/home");
      }
    } catch (error) {
      console.error("Error in handleStartNow:", error);
      // Fallback navigation
      navigate("/home");
    }
  };

  return (
    <section className="welcome-hero">
      <div className="hero-content">
        <div className="hero-left">
          
          <h1 className="hero-title">
            Ghi nhớ 1000 từ vựng<br />
            trong 1 tháng
          </h1>
          <p className="hero-subtitle">
            Học đúng thời điểm vàng giúp bạn học ít vẫn dễ dàng nhớ ngàn từ vựng
          </p>
          <div className="hero-rating">
            <span className="rating-stars">4,7/5 điểm</span>
            <span className="rating-text">với 30,000+ đánh giá</span>
          </div>
          <button 
            className="hero-cta-btn"
            onClick={handleStartNow}
          >
            Bắt đầu ngay
          </button>
        </div>
        <div className="hero-right">
          <div className="hero-images">
            <img 
              src={welcomeImage} 
              alt="Catalunya English Welcome" 
              className="hero-image main-image"
            />
            <img 
              src={mochiWelcome} 
              alt="Mochi Welcome" 
              className="hero-image mochi-welcome"
            />
            <img 
              src={mochiLoading} 
              alt="Mochi Loading" 
              className="hero-image mochi-loading"
            />
          </div>
        </div>
      </div>
    </section>
  );
}


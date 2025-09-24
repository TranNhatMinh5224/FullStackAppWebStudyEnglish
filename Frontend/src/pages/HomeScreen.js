import React, { useState, useEffect } from "react";
import "./HomeScreen.css";
import { images } from "../assets/images";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import LoginModal from "../components/LoginModal";

const HomeScreen = () => {
  const { isLoggedIn, isGuest, user, logout } = useAuth();
  const [showUserDropdown, setShowUserDropdown] = useState(false);
  const [showLoginModal, setShowLoginModal] = useState(false);
  const navigate = useNavigate();

  // Redirect to welcome if neither logged in nor guest
  useEffect(() => {
    if (!isLoggedIn && !isGuest) {
      navigate("/");
    }
  }, [isLoggedIn, isGuest, navigate]);

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  const handleLearnClick = () => {
    if (isGuest) {
      setShowLoginModal(true);
    } else {
      // Navigate to learning page or start learning
      console.log("Start learning...");
    }
  };

  const handleAvatarClick = () => {
    if (isGuest) {
      setShowLoginModal(true);
    } else {
      setShowUserDropdown(!showUserDropdown);
    }
  };

  // Don't render anything if not logged in or guest (will redirect anyway)
  if (!isLoggedIn && !isGuest) {
    return null;
  }

  return (
    <div className="home-container">
      {/* Header với navigation */}
      <header className="home-header">
        <div className="header-content">
          <div className="brand-section">
            <img src={images.logo} alt="Logo" className="logo" />
            <span className="brand">Catalunya English</span>
          </div>
          
          <div className="header-right">
            <nav className="nav-menu">
              <Link to="/review" className="nav-item">Ôn tập</Link>
              <Link to="/learn" className="nav-item active">Học từ mới</Link>
              <Link to="/exercise" className="nav-item">Bài tập</Link>
              <Link to="/offers" className="nav-item">Ưu đãi</Link>
            </nav>
            
            <div className="user-profile-area">
              <div 
                className="profile-avatar" 
                onClick={handleAvatarClick}
              >
                <img src={images.logo2} alt="Avatar" className="avatar-img" />
                {isLoggedIn && <span className="user-name">{user.name}</span>}
              </div>
              
              {showUserDropdown && (
                <div className="profile-dropdown">
                  {isLoggedIn ? (
                    <>
                      <div className="user-info">
                        <div className="user-name-full">{user.name}</div>
                        <div className="user-email">{user.email}</div>
                      </div>
                      <hr className="dropdown-divider" />
                      <button className="dropdown-item logout-btn" onClick={handleLogout}>
                        Đăng xuất
                      </button>
                    </>
                  ) : (
                    <>
                      <Link to="/login" className="dropdown-item" onClick={() => setShowUserDropdown(false)}>
                        Đăng nhập
                      </Link>
                      <Link to="/register" className="dropdown-item" onClick={() => setShowUserDropdown(false)}>
                        Tạo tài khoản
                      </Link>
                    </>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </header>

      {/* Layout chính */}
      <div className="main-layout">
        {/* Phần nội dung chính */}
        <div className="center-section">
          <div className="content-box">
            <img src={images.mascot1} alt="Mascot" className="mascot-big" />
            <p className="slogan">Hãy chăm chỉ học từ mới mỗi ngày bạn nhé ❤️</p>
            <button className="btn-learn" onClick={handleLearnClick}>
              Bắt đầu học ngay
            </button>
          </div>
        </div>

        {/* Sidebar với thông tin */}
        <aside className="sidebar">
          {/* Thống kê học tập */}
          <div className="stat-card yellow">
            <p>Bạn đã học được</p>
            <h3>0 từ</h3>
          </div>

          <div className="stat-card green">
            <p>Học liên tục</p>
            <h3>0 ngày</h3>
          </div>

          {/* Progress section */}
          <div className="progress-section">
            <div className="progress-title">Tiến độ hôm nay</div>
            <div className="progress-bar">
              <div className="progress-fill"></div>
            </div>
            <div className="progress-text">5/20 từ mới</div>
          </div>

          {/* Achievement badges */}
          <div className="achievements">
            <div className="achievement-badge">
              <div className="achievement-icon">🏆</div>
              <div className="achievement-text">Người mới</div>
            </div>
            <div className="achievement-badge">
              <div className="achievement-icon">⭐</div>
              <div className="achievement-text">Siêng năng</div>
            </div>
            <div className="achievement-badge">
              <div className="achievement-icon">🔥</div>
              <div className="achievement-text">Streak</div>
            </div>
            <div className="achievement-badge">
              <div className="achievement-icon">💎</div>
              <div className="achievement-text">VIP</div>
            </div>
          </div>
        </aside>
      </div>

      {/* Login Modal */}
      <LoginModal 
        isOpen={showLoginModal} 
        onClose={() => setShowLoginModal(false)}
      />
    </div>
  );
};

export default HomeScreen;

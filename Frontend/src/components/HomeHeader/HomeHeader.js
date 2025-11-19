import React, { useState, useEffect, useRef } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { images } from "../../assets/images";
import "./HomeHeader.css";

const HomeHeader = ({ activeTab, onTabChange, onShowLoginModal }) => {
  const { isLoggedIn, isGuest, user, logout } = useAuth();
  const [showUserDropdown, setShowUserDropdown] = useState(false);
  const navigate = useNavigate();
  const dropdownRef = useRef(null);

  // Debug: Log user data changes
  useEffect(() => {
    console.log('[HomeHeader] User data:', {
      isLoggedIn,
      isGuest,
      user,
      firstName: user?.firstName,
      lastName: user?.lastName,
      email: user?.email,
      userId: user?.userId
    });
  }, [user, isLoggedIn, isGuest]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setShowUserDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  const handleAvatarClick = () => {
    if (isGuest) {
      onShowLoginModal();
    } else {
      // Toggle dropdown cho tất cả trường hợp
      setShowUserDropdown(!showUserDropdown);
    }
  };

  const handleUpdateProfile = () => {
    setShowUserDropdown(false);
    navigate("/profile/update");
  };

  const handleTabClick = (tab) => {
    onTabChange(tab);
    setShowUserDropdown(false);
  };

  return (
    <header className="home-header">
      <div className="header-content">
        {/* Logo và brand - góc trái */}
        <div className="brand-section">
          <img src={images.logo} alt="Logo" className="logo" />
          <span className="brand">Catalunya English</span>
        </div>
        
        {/* Navigation menu - giữa */}
        
        <nav className="nav-menu">
          <button 
            className={`nav-item ${activeTab === 'review' ? 'active' : ''}`}
            onClick={() => handleTabClick('review')}
          >
            Ôn tập
          </button>
          <button 
            className={`nav-item ${activeTab === 'learn' ? 'active' : ''}`}
            onClick={() => handleTabClick('learn')}
          >
            Học từ mới
          </button>
          <Link to="/exercise" className="nav-item">Bài tập</Link>
          <Link to="/offers" className="nav-item">Ưu đãi</Link>
        </nav>
      

        {/* User profile - góc phải */}
        <div className="user-profile-area" ref={dropdownRef}>
          <div 
            className="profile-avatar" 
            onClick={handleAvatarClick}
          >
            <img src={images.logo2} alt="Avatar" className="avatar-img" />
            {isLoggedIn && (
              <span className="user-name">
                {user?.firstName} {user?.lastName}
              </span>
            )}
          </div>
          
          {/* Dropdown chỉ hiện khi click cho tất cả user */}
          {showUserDropdown && (
            <div className="profile-dropdown">
              {isLoggedIn ? (
                <>
                  <div className="user-info">
                    <div className="user-name-full">{user?.firstName} {user?.lastName}</div>
                    <div className="user-email">{user?.email}</div>
                  </div>
                  <hr className="dropdown-divider" />
                  <button className="dropdown-item profile-btn" onClick={handleUpdateProfile}>
                    Cập nhật thông tin
                  </button>
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
    </header>
  );
};

export default HomeHeader;
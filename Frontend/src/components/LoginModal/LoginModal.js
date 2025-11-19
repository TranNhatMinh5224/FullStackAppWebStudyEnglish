import React from 'react';
import { useNavigate } from 'react-router-dom';
import './LoginModal.css';

const LoginModal = ({ isOpen, onClose }) => {
  const navigate = useNavigate();

  if (!isOpen) return null;

  const handleLoginClick = () => {
    onClose();
    navigate('/login');
  };

  const handleRegisterClick = () => {
    onClose();
    navigate('/register');
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>
          ×
        </button>
        
        <div className="modal-body">
          <h3 className="modal-title">Tạo tài khoản để lưu kết quả học tập của bạn nhé ❤️</h3>
          
          <div className="modal-buttons">
            <button className="modal-btn login-btn" onClick={handleLoginClick}>
              Đăng nhập
            </button>
            <button className="modal-btn register-btn" onClick={handleRegisterClick}>
              Tạo tài khoản
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginModal;
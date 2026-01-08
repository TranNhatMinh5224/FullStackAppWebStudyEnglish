import React from "react";
import { useNavigate } from "react-router-dom";
import "./LoginRequiredModal.css";
import { FaLock } from "react-icons/fa";
import { ROUTE_PATHS } from "../../../Routes/Paths";

export default function LoginRequiredModal({ isOpen, onClose }) {
    const navigate = useNavigate();

    if (!isOpen) return null;

    const handleLogin = () => {
        onClose();
        navigate(ROUTE_PATHS.LOGIN);
    };

    return (
        <div className="modal-overlay login-required-modal-overlay" onClick={onClose}>
            <div className="modal-content login-required-modal-content" onClick={(e) => e.stopPropagation()}>
                <div className="login-required-modal-header">
                    <FaLock className="login-required-icon" />
                    <h2 className="login-required-modal-title">Yêu cầu đăng nhập</h2>
                </div>
                
                <div className="login-required-modal-body">
                    <p className="login-required-message">
                        Vui lòng đăng nhập để sử dụng tính năng này.
                    </p>
                </div>

                <div className="login-required-modal-footer">
                    <button
                        type="button"
                        className="modal-btn modal-btn-cancel"
                        onClick={onClose}
                    >
                        Hủy
                    </button>
                    <button
                        type="button"
                        className="modal-btn login-required-btn"
                        onClick={handleLogin}
                    >
                        Đăng nhập
                    </button>
                </div>
            </div>
        </div>
    );
}


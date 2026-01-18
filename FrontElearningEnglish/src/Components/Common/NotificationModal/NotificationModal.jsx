import React, { useEffect } from "react";
import "./NotificationModal.css";
import { FaCheckCircle, FaTimesCircle, FaInfoCircle } from "react-icons/fa";

export default function NotificationModal({ 
    isOpen, 
    onClose, 
    type = "success", // "success", "error", "info"
    title,
    message,
    autoClose = true,
    autoCloseDelay = 3000
}) {
    useEffect(() => {
        if (isOpen && autoClose) {
            const timer = setTimeout(() => {
                onClose();
            }, autoCloseDelay);
            return () => clearTimeout(timer);
        }
    }, [isOpen, autoClose, autoCloseDelay, onClose]);

    if (!isOpen) return null;

    const getIcon = () => {
        switch (type) {
            case "success":
                return <FaCheckCircle className="notification-modal-icon notification-modal-icon--success" />;
            case "error":
                return <FaTimesCircle className="notification-modal-icon notification-modal-icon--error" />;
            case "info":
                return <FaInfoCircle className="notification-modal-icon notification-modal-icon--info" />;
            default:
                return <FaInfoCircle className="notification-modal-icon notification-modal-icon--info" />;
        }
    };

    const getTitle = () => {
        if (title) return title;
        switch (type) {
            case "success":
                return "Thành công";
            case "error":
                return "Lỗi";
            case "info":
                return "Thông báo";
            default:
                return "Thông báo";
        }
    };

    return (
        <div className="modal-overlay notification-modal-overlay" onClick={onClose}>
            <div className="modal-content notification-modal-content" onClick={(e) => e.stopPropagation()}>
                <div className={`notification-modal-header notification-modal-header--${type}`}>
                    <div className={`notification-modal-icon-wrapper notification-modal-icon-wrapper--${type}`}>
                        {getIcon()}
                    </div>
                    <h2 className="notification-modal-title">{getTitle()}</h2>
                </div>
                
                <div className="notification-modal-body">
                    <p className="notification-message">{message}</p>
                </div>

                <div className="notification-modal-footer">
                    <button
                        type="button"
                        className={`notification-modal-btn notification-modal-btn--${type}`}
                        onClick={onClose}
                    >
                        Đóng
                    </button>
                </div>
            </div>
        </div>
    );
}


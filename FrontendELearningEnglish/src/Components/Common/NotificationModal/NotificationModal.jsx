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
                return <FaCheckCircle className="notification-icon notification-icon-success" />;
            case "error":
                return <FaTimesCircle className="notification-icon notification-icon-error" />;
            case "info":
                return <FaInfoCircle className="notification-icon notification-icon-info" />;
            default:
                return <FaInfoCircle className="notification-icon notification-icon-info" />;
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
                <div className="notification-modal-header">
                    {getIcon()}
                    <h2 className="notification-modal-title">{getTitle()}</h2>
                </div>
                
                <div className="notification-modal-body">
                    <p className="notification-message">{message}</p>
                </div>

                <div className="notification-modal-footer">
                    <button
                        type="button"
                        className={`modal-btn notification-btn notification-btn-${type}`}
                        onClick={onClose}
                    >
                        Đóng
                    </button>
                </div>
            </div>
        </div>
    );
}


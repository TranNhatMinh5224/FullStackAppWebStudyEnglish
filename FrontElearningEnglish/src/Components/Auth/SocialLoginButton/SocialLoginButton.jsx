import React from "react";
import "./SocialLoginButton.css";

export default function SocialLoginButton({
    type,
    icon: Icon,
    text,
    onClick,
    disabled = false,
    loading = false,
}) {
    return (
        <button
            className={`social-login-btn ${type} d-flex align-items-center justify-content-center`}
            onClick={onClick}
            disabled={disabled || loading}
            type="button"
        >
            <Icon className="social-icon" />
            <span>{loading ? "Đang xử lý..." : text}</span>
        </button>
    );
}


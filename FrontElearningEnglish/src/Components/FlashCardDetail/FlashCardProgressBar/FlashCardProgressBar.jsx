import React from "react";
import { FaTimes } from "react-icons/fa";
import "./FlashCardProgressBar.css";

export default function FlashCardProgressBar({ current, total, onClose, variant }) {
    const percentage = total > 0 ? Math.round((current / total) * 100) : 0;
    const isReviewVariant = variant === "review";

    return (
        <div className={`flashcard-progress-bar ${isReviewVariant ? "review-variant" : ""}`}>
            <div className="progress-container d-flex flex-column">
                <div className="progress-top-row d-flex justify-content-between align-items-center">
                    {!isReviewVariant && <span className="progress-label">Số lượng</span>}
                    <span className="progress-count">{current}/{total}</span>
                </div>
                <div className="progress-bottom-row d-flex align-items-center">
                    {onClose && (
                        <button
                            className="progress-close-btn d-flex align-items-center justify-content-center"
                            onClick={onClose}
                            title="Thoát"
                        >
                            <FaTimes />
                        </button>
                    )}
                    <div className="progress-track">
                        <div className="progress-fill" style={{ width: `${percentage}%` }}></div>
                    </div>
                </div>
            </div>
        </div>
    );
}


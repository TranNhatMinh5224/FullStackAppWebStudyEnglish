import React from "react";
import "./UpgradeCard.css";

export default function UpgradeCard({
    teacherPackageId,
    packageType,
    title,
    description,
    price,
    isSelected = false,
    onMouseEnter,
    onMouseLeave,
    onUpgradeClick,
    onCardClick,
}) {
    const handleCardClick = (e) => {
        // Nếu click vào nút "Nâng cấp", không mở modal
        if (e.target.closest('.upgrade-btn')) {
            return;
        }
        // Click vào card → mở modal chi tiết
        onCardClick?.(teacherPackageId);
    };

    return (
        <div
            className={`upgrade-card ${isSelected ? "selected" : ""}`}
            onMouseEnter={onMouseEnter}
            onMouseLeave={onMouseLeave}
            onClick={handleCardClick}
        >
            <div className="upgrade-card-content">
                <h3>{title}</h3>
                <p>{description}</p>
            </div>
            <div className="upgrade-card-footer">
                <strong>{price}</strong>
                <button
                    className="upgrade-btn"
                    onClick={(e) => {
                        e.stopPropagation(); // Ngăn event bubble lên card
                        onUpgradeClick?.(e, teacherPackageId, packageType);
                    }}
                >
                    Nâng cấp
                </button>
            </div>
        </div>
    );
}


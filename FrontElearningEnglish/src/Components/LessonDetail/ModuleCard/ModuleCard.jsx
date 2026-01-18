import React from "react";
import { FaMicrophone } from "react-icons/fa";
import {
    FaBookOpen,
    FaFileAlt,
    FaEdit
} from "react-icons/fa";
import ImageWithIconFallback from "../../Common/ImageWithIconFallback/ImageWithIconFallback";
import "./ModuleCard.css";

export default function ModuleCard({ module, onClick, onPronunciationClick }) {
    const {
        name = "Module",
        contentType = 1, // 1=Lecture, 2=FlashCard, 3=Assessment
        contentTypeName = "Lecture",
        isCompleted = false,
        description = "",
        isPronunciationCompleted = false, // Thông tin về pronunciation completion
        imageUrl = "",
        ImageUrl = "",
    } = module || {};

    const finalName = name || "Module";
    const finalContentType = contentType;
    const finalContentTypeName = contentTypeName || "Lecture";
    const finalIsCompleted = isCompleted;
    const finalDescription = description || "";
    const finalIsPronunciationCompleted = isPronunciationCompleted || false;

    // Check if this is a flashcard module
    const isFlashCard = finalContentType === 2 || finalContentTypeName.toLowerCase().includes("flashcard");

    // Get icon and class name based on content type (1=Lecture, 2=FlashCard, 3=Assessment)
    const getIconConfig = (type, typeName) => {
        const typeLower = (typeName || "").toLowerCase();
        if (type === 1 || typeLower.includes("lecture")) {
            return { icon: <FaBookOpen />, className: "lecture" };
        } else if (type === 2 || typeLower.includes("flashcard") || typeLower.includes("flash")) {
            return { icon: <FaFileAlt />, className: "flashcard" };
        } else if (type === 3 || typeLower.includes("assessment") || typeLower.includes("assignment") || typeLower.includes("essay")) {
            return { icon: <FaEdit />, className: "assignment" };
        }
        return { icon: <FaBookOpen />, className: "lecture" };
    };

    const iconConfig = getIconConfig(finalContentType, finalContentTypeName);

    // Handle card click - navigate to module content
    const handleCardClick = (e) => {
        // Don't navigate if clicking on pronunciation button or its children
        if (e.target.closest('.pronunciation-btn')) {
            return;
        }
        if (onClick) {
            onClick();
        }
    };

    // Handle pronunciation button click - navigate to pronunciation
    const handlePronunciationClick = (e) => {
        e.stopPropagation(); // Prevent card click
        // Only allow click if flashcard is completed
        if (!finalIsCompleted) {
            return;
        }
        if (onPronunciationClick) {
            onPronunciationClick();
        }
    };

    return (
        <div
            className={`module-card ${finalIsCompleted ? "completed" : ""}`}
            onClick={handleCardClick}
        >
            <div className="module-icon-wrapper">
                <ImageWithIconFallback
                    imageUrl={imageUrl}
                    ImageUrl={ImageUrl}
                    icon={<div className={`module-icon ${iconConfig.className}`}>{iconConfig.icon}</div>}
                    alt={finalName}
                    className={`module-image ${iconConfig.className}`}
                    iconClassName={`module-icon ${iconConfig.className}`}
                    imageKey={module?.moduleId || module?.ModuleId}
                />
            </div>
            <div className="module-content">
                <h3 className="module-title">{finalName}</h3>
                {finalDescription && (
                    <p className="module-description">{finalDescription}</p>
                )}
            </div>
            <div className="module-actions">
                {isFlashCard && (
                    <button
                        className={`pronunciation-btn ${!finalIsCompleted
                                ? "pronunciation-disabled"
                                : finalIsPronunciationCompleted
                                    ? "pronunciation-completed"
                                    : "pronunciation-pending"
                            }`}
                        onClick={handlePronunciationClick}
                    >
                        <FaMicrophone />
                        <span>pronunciation</span>
                    </button>
                )}
            </div>
        </div>
    );
}


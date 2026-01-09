import React from "react";
import { FaCheckCircle, FaPlay, FaMicrophone } from "react-icons/fa";
import { mochiModuleTeacher } from "../../../Assets/Logo";
import "./ModuleCard.css";

export default function ModuleCard({ module, onClick, onPronunciationClick }) {
    // Debug: log module data to see what's coming from API
    console.log("ModuleCard - module data:", module);
    
    const {
        moduleId,
        name = "Module",
        Name,
        contentType = 1, // 1=Lecture, 2=FlashCard, 3=Assessment
        ContentType,
        contentTypeName = "Lecture",
        ContentTypeName,
        isCompleted = false,
        IsCompleted,
        description = "",
        Description,
        startedAt = null,
        StartedAt,
        isPronunciationCompleted = false,
        IsPronunciationCompleted,
        imageUrl = null,
        ImageUrl,
    } = module || {};

    const finalName = name || Name || "Module";
    const finalContentType = contentType || ContentType || 1;
    const finalContentTypeName = contentTypeName || ContentTypeName || "Lecture";
    const finalIsCompleted = isCompleted || IsCompleted || false;
    const finalDescription = description || Description || "";
    const finalIsPronunciationCompleted = isPronunciationCompleted || IsPronunciationCompleted || false;
    const finalImageUrl = imageUrl || ImageUrl || mochiModuleTeacher; // Dùng ảnh mặc định nếu không có
    const finalStartedAt = startedAt || StartedAt || null;
    
    console.log("ModuleCard - finalImageUrl:", finalImageUrl);

    // Determine module status
    const isInProgress = !finalIsCompleted && finalStartedAt !== null;
    const isNotStarted = !finalIsCompleted && finalStartedAt === null;

    // Check if this is a flashcard module
    const isFlashCard = finalContentType === 2 || finalContentTypeName.toLowerCase().includes("flashcard");

    // Get button text and action
    const getButtonConfig = () => {
        if (finalIsCompleted) {
            return {
                text: "Đã học",
                icon: <FaCheckCircle />,
                className: "completed-btn",
            };
        } else if (isInProgress) {
            return {
                text: "Tiếp tục",
                icon: <FaPlay />,
                className: "continue-btn",
            };
        } else {
            return {
                text: "Bắt đầu",
                icon: <FaPlay />,
                className: "start-btn",
            };
        }
    };

    const buttonConfig = getButtonConfig();

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
                <div className="module-image">
                    <img 
                        src={finalImageUrl} 
                        alt={finalName} 
                        className="module-thumbnail"
                    />
                </div>
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


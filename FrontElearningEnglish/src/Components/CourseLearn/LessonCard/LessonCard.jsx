import React from "react";
import { FaCheckCircle, FaBook } from "react-icons/fa";
import { useAssets } from "../../../Context/AssetContext";
import ImageWithIconFallback from "../../Common/ImageWithIconFallback/ImageWithIconFallback";
import "./LessonCard.css";

export default function LessonCard({ lesson, orderNumber, onClick }) {
    const { getDefaultLessonImage } = useAssets();
    const {
        lessonId,
        LessonId,
        title = "Bài học",
        Title,
        imageUrl,
        ImageUrl,
        isCompleted = false,
        IsCompleted = false,
        orderIndex,
        OrderIndex,
        description,
        Description,
    } = lesson || {};

    const finalLessonId = lessonId || LessonId;
    const finalTitle = title || Title || "Bài học";
    const finalIsCompleted = isCompleted || IsCompleted;
    const finalOrderIndex = orderIndex || OrderIndex;
    const finalDescription = description || Description;
    const displayOrder = orderNumber || finalOrderIndex || 1;
    const defaultImage = getDefaultLessonImage();
    const finalImageUrl = (imageUrl || ImageUrl) || defaultImage;

    const handleClick = () => {
        if (onClick && finalLessonId) {
            onClick(finalLessonId);
        }
    };

    return (
        <div className={`lesson-card ${finalIsCompleted ? "completed" : ""}`} onClick={handleClick}>
            <div className="lesson-image-wrapper">
                <ImageWithIconFallback
                    imageUrl={finalImageUrl}
                    icon={
                        <div className="lesson-image-placeholder">
                            <FaBook size={32} />
                            <span>{displayOrder}</span>
                        </div>
                    }
                    alt={finalTitle}
                    className="lesson-image"
                    iconClassName="lesson-image-placeholder"
                    imageKey={finalLessonId}
                />
                {finalIsCompleted && (
                    <div className="lesson-completed-badge">
                        <FaCheckCircle />
                    </div>
                )}
            </div>
            <div className="lesson-info">
                <h3 className="lesson-title">{finalTitle}</h3>
                <span className="lesson-order">{displayOrder}. {finalDescription || finalTitle}</span>
            </div>
        </div>
    );
}


import React from "react";
import "./LessonDetailHeader.css";

export default function LessonDetailHeader({ title, description, onBackClick }) {
    return (
        <>
            <div className="lesson-detail-header-banner">
                <h1>{title}</h1>
            </div>
            {description && (
                <p className="lesson-description">{description}</p>
            )}
        </>
    );
}


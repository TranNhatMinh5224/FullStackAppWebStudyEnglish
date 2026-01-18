import React from "react";
import "./RegisteredCourseCard.css";
import { useAssets } from "../../../Context/AssetContext";

export default function RegisteredCourseCard({ course, onContinue }) {
    const { getDefaultCourseImage } = useAssets();
    const {
        title = "IELTS 6.5",
        imageUrl,
        progress = 40,
    } = course || {};
    
    const displayImageUrl = (imageUrl && imageUrl.trim() !== "") 
        ? imageUrl 
        : getDefaultCourseImage() || "https://images.unsplash.com/photo-1507525428034-b723cf961d3e";

    return (
        <div className="registered-course-card">
            <img src={displayImageUrl} alt={title} />
            <div className="course-info">
                <h3>{title}</h3>
                <div className="progress-wrapper">
                    <div className="progress">
                        <div className="progress-bar" style={{ width: `${progress}%` }} />
                    </div>
                    <span className="progress-text">{progress}%</span>
                </div>
                <button className="continue-btn" onClick={() => onContinue?.(course)}>
                    Tiếp tục học
                </button>
            </div>
        </div>
    );
}


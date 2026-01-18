import React from "react";
import { useNavigate } from "react-router-dom";
import { FaGraduationCap } from "react-icons/fa";
import { useAssets } from "../../../Context/AssetContext";
import ImageWithIconFallback from "../../Common/ImageWithIconFallback/ImageWithIconFallback";
import "./CourseCard.css";

export default function CourseCard({ course }) {
    const navigate = useNavigate();
    const { getDefaultCourseImage } = useAssets();
    const {
        id,
        courseId,
        CourseId,
        title = "IELTS 6.5",
        Title,
        imageUrl,
        ImageUrl,
    } = course || {};

    const finalId = id || courseId || CourseId;
    const finalTitle = title || Title || "IELTS 6.5";
    const defaultImage = getDefaultCourseImage();
    const finalImageUrl = (imageUrl || ImageUrl) || defaultImage;

    const handleClick = () => {
        if (finalId) {
            navigate(`/course/${finalId}`);
        }
    };

    return (
        <div className="course-card" onClick={handleClick}>
            <ImageWithIconFallback
                imageUrl={finalImageUrl}
                icon={<FaGraduationCap size={48} />}
                alt={finalTitle}
                className="course-image"
                iconClassName="course-icon-fallback"
                imageKey={finalId}
            />
            <div className="course-info">
                <h3>{finalTitle}</h3>
            </div>
        </div>
    );
}


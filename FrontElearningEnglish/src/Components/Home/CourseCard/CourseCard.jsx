import React from "react";
import { useNavigate } from "react-router-dom";
import "./CourseCard.css";

export default function CourseCard({ course }) {
    const navigate = useNavigate();
    const {
        id,
        title = "IELTS 6.5",
        imageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e",
    } = course || {};

    const handleClick = () => {
        if (id) {
            navigate(`/course/${id}`);
        }
    };

    return (
        <div className="course-card" onClick={handleClick}>
            <img src={imageUrl} alt={title} />
            <div className="course-info">
                <h3>{title}</h3>
            </div>
        </div>
    );
}


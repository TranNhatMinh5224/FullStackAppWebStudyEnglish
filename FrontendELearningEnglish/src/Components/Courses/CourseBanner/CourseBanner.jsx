import React from "react";
import "./CourseBanner.css";
import { dainganha as dainganhaImage } from "../../../Assets";

export default function CourseBanner({ title, description, imageUrl }) {
    const bannerImage = imageUrl && imageUrl.trim() !== "" 
        ? imageUrl 
        : dainganhaImage;

    return (
        <div className="course-banner">
            <div className="course-banner-background" style={{ backgroundImage: `url(${bannerImage})` }}>
                <div className="course-banner-overlay"></div>
                <div className="course-banner-content">
                    <h1 className="course-banner-title">{title}</h1>
                    {description && (
                        <p className="course-banner-description">{description}</p>
                    )}
                </div>
            </div>
        </div>
    );
}


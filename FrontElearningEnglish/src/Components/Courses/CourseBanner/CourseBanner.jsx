import React from "react";
import "./CourseBanner.css";
import { useAssets } from "../../../Context/AssetContext";

export default function CourseBanner({ title, description, imageUrl }) {
    const { getDefaultCourseImage } = useAssets();
    const bannerImage = imageUrl && imageUrl.trim() !== "" 
        ? imageUrl 
        : getDefaultCourseImage();

    return (
        <div className="course-banner">
            <div className="course-banner-background d-flex align-items-center justify-content-center" style={{ backgroundImage: `url(${bannerImage})` }}>
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


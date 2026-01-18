import React, { useState, useEffect } from "react";
import "./ImageWithIconFallback.css";

/**
 * Component hiển thị ảnh với fallback tự động sang icon React
 * @param {string} imageUrl - URL của ảnh (optional)
 * @param {React.ReactElement} icon - Icon React để hiển thị khi không có ảnh
 * @param {string} alt - Alt text cho ảnh
 * @param {string} className - CSS class name
 * @param {string} iconClassName - CSS class name cho icon wrapper
 * @param {object} style - Inline styles
 * @param {function} onError - Callback khi ảnh lỗi
 * @param {any} imageKey - Key để reset error state khi thay đổi
 */
export default function ImageWithIconFallback({
    imageUrl,
    ImageUrl,
    icon,
    alt = "",
    className = "",
    iconClassName = "",
    style = {},
    onError,
    imageKey,
    ...props
}) {
    const [imageError, setImageError] = useState(false);
    const finalImageUrl = imageUrl || ImageUrl || "";

    // Reset image error when imageUrl or imageKey changes
    useEffect(() => {
        setImageError(false);
    }, [finalImageUrl, imageKey]);

    const handleImageError = (e) => {
        setImageError(true);
        if (onError) {
            onError(e);
        }
    };

    // If no image URL or image failed to load, show icon
    if (!finalImageUrl || imageError) {
        return (
            <div 
                className={`image-with-icon-fallback ${iconClassName} ${className}`}
                style={style}
                {...props}
            >
                {icon}
            </div>
        );
    }

    // Show image if available
    return (
        <img
            src={finalImageUrl}
            alt={alt}
            className={`image-with-icon-fallback-image ${className}`}
            style={style}
            onError={handleImageError}
            {...props}
        />
    );
}

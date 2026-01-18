import React from "react";
import { useNavigate } from "react-router-dom";
import { FaHome } from "react-icons/fa";
import "./Breadcrumb.css";

/**
 * Breadcrumb Component - Tái sử dụng được
 * @param {Array} items - Mảng các item breadcrumb: [{ label, path, isCurrent? }]
 * @param {Boolean} showHomeIcon - Hiển thị icon home ở item đầu tiên
 */
const Breadcrumb = ({ items = [], showHomeIcon = true }) => {
    const navigate = useNavigate();

    if (!items || items.length === 0) {
        return null;
    }

    return (
        <nav className="custom-breadcrumb">
            {items.map((item, index) => {
                const isLast = index === items.length - 1;
                const isFirst = index === 0;
                const isCurrent = item.isCurrent !== undefined ? item.isCurrent : isLast;

                return (
                    <React.Fragment key={index}>
                        {index > 0 && <span className="breadcrumb-separator">/</span>}
                        {isCurrent ? (
                            <span className="breadcrumb-item breadcrumb-current">
                                {isFirst && showHomeIcon && <FaHome className="breadcrumb-icon" />}
                                <span>{item.label}</span>
                            </span>
                        ) : (
                            <button
                                onClick={() => item.path && navigate(item.path)}
                                className="breadcrumb-item breadcrumb-link"
                            >
                                {isFirst && showHomeIcon && <FaHome className="breadcrumb-icon" />}
                                <span>{item.label}</span>
                            </button>
                        )}
                    </React.Fragment>
                );
            })}
        </nav>
    );
};

Breadcrumb.displayName = "Breadcrumb";

export default Breadcrumb;

import React from "react";
import { useNavigate } from "react-router-dom";
import { FaBars, FaHome, FaComments } from "react-icons/fa";
import "./LectureHeader.css";

const LectureHeader = ({ 
    sidebarCollapsed, 
    onToggleSidebar, 
    courseId, 
    lessonId, 
    courseTitle, 
    lessonTitle, 
    moduleName 
}) => {
    const navigate = useNavigate();

    return (
        <header className="lecture-header d-flex align-items-center justify-content-between">
            <div className="lecture-header-left d-flex align-items-center flex-grow-1">
                <button 
                    className="sidebar-toggle-btn"
                    onClick={onToggleSidebar}
                    aria-label="Toggle sidebar"
                >
                    <FaBars />
                </button>
                <nav className="lecture-breadcrumb d-flex align-items-center flex-grow-1">
                    <button 
                        onClick={() => navigate("/my-courses")} 
                        className="breadcrumb-item breadcrumb-link btn btn-link p-0 border-0 text-decoration-none"
                    >
                        <FaHome className="breadcrumb-icon" />
                        <span className="small">Khóa học của tôi</span>
                    </button>
                    <span className="breadcrumb-separator">/</span>
                    <button 
                        onClick={() => navigate(`/course/${courseId}`)} 
                        className="breadcrumb-item breadcrumb-link btn btn-link p-0 border-0 text-decoration-none"
                    >
                        <span className="small">{courseTitle}</span>
                    </button>
                    <span className="breadcrumb-separator">/</span>
                    <button 
                        onClick={() => navigate(`/course/${courseId}/learn`)} 
                        className="breadcrumb-item breadcrumb-link btn btn-link p-0 border-0 text-decoration-none d-none d-sm-inline-flex"
                    >
                        <span className="small">Lesson</span>
                    </button>
                    <span className="breadcrumb-separator d-none d-sm-inline">/</span>
                    <button 
                        onClick={() => navigate(`/course/${courseId}/lesson/${lessonId}`)} 
                        className="breadcrumb-item breadcrumb-link btn btn-link p-0 border-0 text-decoration-none d-none d-md-inline-flex"
                    >
                        <span className="small">{lessonTitle}</span>
                    </button>
                    <span className="breadcrumb-separator d-none d-md-inline">/</span>
                    <span className="breadcrumb-item breadcrumb-current small d-none d-lg-inline">{moduleName}</span>
                </nav>
            </div>
            <div className="lecture-header-right d-flex align-items-center">
                <button className="discussion-btn btn btn-primary btn-sm">
                    <FaComments />
                    <span className="d-none d-sm-inline">Xem thảo luận</span>
                </button>
            </div>
        </header>
    );
};

LectureHeader.displayName = "LectureHeader";

export default LectureHeader;

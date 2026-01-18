import React from "react";
import MarkdownViewer from "../MarkdownViewer/MarkdownViewer";
import "./LectureContent.css";

const LectureContent = ({ lecture, loading, error }) => {
    if (loading) {
        return (
            <div className="lecture-content-inner">
                <div className="loading-message">Đang tải nội dung...</div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="lecture-content-inner">
                <div className="error-message">{error}</div>
            </div>
        );
    }

    if (!lecture) {
        return (
            <div className="lecture-content-inner">
                <div className="no-lecture-message">
                    <p>Chọn một bài giảng từ danh sách bên trái để xem nội dung</p>
                </div>
            </div>
        );
    }

    return (
        <div className="lecture-content-inner">
            <MarkdownViewer lecture={lecture} />
        </div>
    );
};

LectureContent.displayName = "LectureContent";

export default LectureContent;

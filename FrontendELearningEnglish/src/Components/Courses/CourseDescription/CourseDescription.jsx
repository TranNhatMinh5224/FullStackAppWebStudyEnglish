import React from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import "./CourseDescription.css";

export default function CourseDescription({ description }) {
    if (!description || description.trim() === "") {
        return (
            <div className="course-description-empty">
                <p>Khóa học này chưa có mô tả chi tiết.</p>
            </div>
        );
    }

    return (
        <div className="course-description-wrapper">
            <div className="course-description-content">
                <ReactMarkdown remarkPlugins={[remarkGfm]}>
                    {description}
                </ReactMarkdown>
            </div>
        </div>
    );
}


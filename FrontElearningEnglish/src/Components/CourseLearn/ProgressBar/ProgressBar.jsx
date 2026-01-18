import React from "react";
import "./ProgressBar.css";

export default function ProgressBar({ completed, total, percentage }) {
    // Đảm bảo percentage không vượt quá 100%
    const safePercentage = Math.min(Math.max(Number(percentage) || 0, 0), 100);

    // Tính lại percentage từ completed/total nếu percentage không hợp lệ
    const calculatedPercentage = total > 0
        ? Math.min(Math.round((completed / total) * 100), 100)
        : 0;

    // Ưu tiên dùng percentage từ props, nếu không hợp lệ thì tính từ completed/total
    const finalPercentage = percentage > 0 ? safePercentage : calculatedPercentage;
    
    // Làm tròn đến 1 chữ số thập phân cho hiển thị
    const displayPercentage = Number(finalPercentage.toFixed(1));

    return (
        <div className="course-progress-bar">
            <div className="progress-header">
                <span className="progress-label">Tiến độ khóa học</span>
                <span className="progress-percentage">{displayPercentage}%</span>
            </div>
            <div className="progress-track">
                <div
                    className="progress-fill"
                    style={{ width: `${finalPercentage}%` }}
                />
            </div>
            <div className="progress-stats">
                <span>{completed}/{total} bài học đã hoàn thành</span>
            </div>
        </div>
    );
}


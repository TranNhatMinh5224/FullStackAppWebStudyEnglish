import React from "react";
import "./CourseSummaryCard.css";
import { FaBook, FaTag, FaUsers } from "react-icons/fa";
import ProgressBar from "../../CourseLearn/ProgressBar/ProgressBar";

export default function CourseSummaryCard({ course, onEnroll, onStartLearning }) {
    // Format price display
    const getPriceDisplay = () => {
        if (course.price === null || course.price === undefined) {
            return null; // Don't show price section
        }
        if (course.price === 0) {
            return "Free";
        }
        // Format price with Vietnamese currency format
        return `${course.price.toLocaleString("vi-VN")}đ`;
    };

    const priceDisplay = getPriceDisplay();

    // Lấy tiến độ từ API - hỗ trợ cả camelCase và PascalCase
    const getProgressData = () => {
        // Hỗ trợ cả camelCase và PascalCase từ API
        const completedLessons = course.completedLessons || course.CompletedLessons || 0;
        const totalLessons = course.totalLessons || course.TotalLessons || 0;
        const progressPercentage = course.progressPercentage || course.ProgressPercentage || 0;

        // Đảm bảo percentage không vượt quá 100%
        const safePercentage = Math.min(Math.max(Number(progressPercentage), 0), 100);

        // Tính lại percentage từ completed/total nếu cần (để đảm bảo chính xác)
        const calculatedPercentage = totalLessons > 0
            ? Math.min(Math.round((completedLessons / totalLessons) * 100), 100)
            : 0;

        // Ưu tiên dùng percentage từ API, nếu không có thì tính từ completed/total
        const finalPercentage = progressPercentage > 0 ? safePercentage : calculatedPercentage;

        return {
            completed: completedLessons,
            total: totalLessons,
            percentage: finalPercentage
        };
    };

    const progressData = course.isEnrolled ? getProgressData() : null;

    return (
        <div className="course-summary-card d-flex flex-column">
            {course.isEnrolled && progressData && (
                <div className="course-progress-section">
                    <ProgressBar
                        completed={progressData.completed}
                        total={progressData.total}
                        percentage={progressData.percentage}
                    />
                </div>
            )}

            <div className="course-summary-stats d-flex flex-column">
                <div className="course-stat-item d-flex align-items-center">
                    <FaBook className="stat-icon" />
                    <div className="stat-content d-flex flex-column">
                        <span className="stat-label">Số lượng bài giảng</span>
                        <span className="stat-value">{course.totalLessons || course.TotalLessons || 0} bài giảng</span>
                    </div>
                </div>

                {course.enrollmentCount !== undefined && (
                    <div className="course-stat-item d-flex align-items-center">
                        <FaUsers className="stat-icon" />
                        <div className="stat-content d-flex flex-column">
                            <span className="stat-label">Số học viên</span>
                            <span className="stat-value">{course.enrollmentCount || 0} học viên</span>
                        </div>
                    </div>
                )}

                {priceDisplay !== null && (
                    <div className="course-stat-item d-flex align-items-center">
                        <FaTag className="stat-icon" />
                        <div className="stat-content d-flex flex-column">
                            <span className="stat-label">Giá khóa học</span>
                            <span className="stat-value">{priceDisplay}</span>
                        </div>
                    </div>
                )}
            </div>

            {course.isEnrolled ? (
                <div className="course-enrolled-actions d-flex flex-column">
                    <button className="course-enrolled-btn">
                        Đã đăng kí
                    </button>
                    <button
                        className="course-start-btn"
                        onClick={onStartLearning}
                    >
                        Vào học ngay
                    </button>
                </div>
            ) : (
                <button
                    className="course-enroll-btn"
                    onClick={onEnroll}
                >
                    Đăng kí ngay
                </button>
            )}
        </div>
    );
}


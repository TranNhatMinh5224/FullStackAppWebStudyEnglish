import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Container } from "react-bootstrap";
import MainHeader from "../../Components/Header/MainHeader";
import Breadcrumb from "../../Components/Common/Breadcrumb/Breadcrumb";
import CourseLearnHeader from "../../Components/CourseLearn/CourseLearnHeader/CourseLearnHeader";
import LessonCard from "../../Components/CourseLearn/LessonCard/LessonCard";
import ProgressBar from "../../Components/CourseLearn/ProgressBar/ProgressBar";
import { lessonService } from "../../Services/lessonService";
import { courseService } from "../../Services/courseService";
import "./CourseLearn.css";

export default function CourseLearn() {
    const { courseId } = useParams();
    const navigate = useNavigate();
    const [lessons, setLessons] = useState([]);
    const [course, setCourse] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);
                setError("");

                // Fetch course info
                const courseResponse = await courseService.getCourseById(courseId);
                if (courseResponse.data?.success && courseResponse.data?.data) {
                    setCourse(courseResponse.data.data);
                }

                // Fetch lessons
                const lessonsResponse = await lessonService.getLessonsByCourseId(courseId);
                if (lessonsResponse.data?.success && lessonsResponse.data?.data) {
                    const lessonsData = lessonsResponse.data.data;
                    // Sort by OrderIndex (handle both camelCase and PascalCase)
                    const sortedLessons = lessonsData.sort((a, b) => {
                        const orderA = a.orderIndex || a.OrderIndex || 0;
                        const orderB = b.orderIndex || b.OrderIndex || 0;
                        return orderA - orderB;
                    });
                    setLessons(sortedLessons);
                } else {
                    setError(lessonsResponse.data?.message || "Không thể tải danh sách bài học");
                }
            } catch (err) {
                console.error("Error fetching course learn data:", err);
                setError("Không thể tải dữ liệu khóa học");
            } finally {
                setLoading(false);
            }
        };

        if (courseId) {
            fetchData();
        }
    }, [courseId]);

    const handleLessonClick = (lessonId) => {
        navigate(`/course/${courseId}/lesson/${lessonId}`);
    };

    // Lấy tiến độ từ API (ưu tiên) hoặc tính từ lessons array
    const getProgressData = () => {
        // Ưu tiên lấy từ course object (từ API getCourseById)
        const apiCompletedLessons = course?.completedLessons || course?.CompletedLessons;
        const apiTotalLessons = course?.totalLessons || course?.TotalLessons;
        const apiProgressPercentage = course?.progressPercentage || course?.ProgressPercentage;

        if (apiCompletedLessons !== undefined && apiTotalLessons !== undefined) {
            // Lấy từ API
            const safePercentage = Math.min(Math.max(Number(apiProgressPercentage) || 0, 0), 100);
            return {
                completed: apiCompletedLessons,
                total: apiTotalLessons,
                percentage: safePercentage > 0 ? safePercentage : (apiTotalLessons > 0 ? Math.round((apiCompletedLessons / apiTotalLessons) * 100) : 0)
            };
        }

        // Fallback: tính từ lessons array
        const completedLessons = lessons.filter(lesson =>
            lesson.isCompleted || lesson.IsCompleted
        ).length;
        const totalLessons = lessons.length;
        const progressPercentage = totalLessons > 0 ? Math.round((completedLessons / totalLessons) * 100) : 0;

        return {
            completed: completedLessons,
            total: totalLessons,
            percentage: progressPercentage
        };
    };

    const progressData = getProgressData();

    if (loading) {
        return (
            <>
                <MainHeader />
                <div className="course-learn-container">
                    <div className="loading-message">Đang tải...</div>
                </div>
            </>
        );
    }

    if (error) {
        return (
            <>
                <MainHeader />
                <div className="course-learn-container">
                    <div className="error-message">{error}</div>
                </div>
            </>
        );
    }

    return (
        <>
            <MainHeader />
            <div className="course-learn-container">
                <Container>
                    <Breadcrumb
                        items={[
                            { label: "Khóa học của tôi", path: "/my-courses" },
                            { label: course?.title || course?.Title || "Khóa học", path: `/course/${courseId}` },
                            { label: "Lesson", isCurrent: true }
                        ]}
                    />
                    <CourseLearnHeader courseTitle={course?.title || course?.Title || "Khóa học"} />

                    {progressData.total > 0 && (
                        <ProgressBar
                            completed={progressData.completed}
                            total={progressData.total}
                            percentage={progressData.percentage}
                        />
                    )}

                    <div className="lessons-list d-flex flex-column">
                        {lessons.length > 0 ? (
                            lessons.map((lesson, index) => {
                                const lessonId = lesson.lessonId || lesson.LessonId;
                                return (
                                    <LessonCard
                                        key={lessonId || index}
                                        lesson={lesson}
                                        orderNumber={index + 1}
                                        onClick={handleLessonClick}
                                    />
                                );
                            })
                        ) : (
                            <div className="no-lessons-message">Chưa có bài học nào</div>
                        )}
                    </div>
                </Container>
            </div>
        </>
    );
}


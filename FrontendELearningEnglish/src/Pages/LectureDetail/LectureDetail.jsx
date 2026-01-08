import React, { useState, useEffect, useCallback, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Container } from "react-bootstrap";
import MainHeader from "../../Components/Header/MainHeader";
import LectureSidebar from "../../Components/LectureDetail/LectureSidebar/LectureSidebar";
import LectureContent from "../../Components/LectureDetail/LectureContent/LectureContent";
import { lectureService } from "../../Services/lectureService";
import { courseService } from "../../Services/courseService";
import { lessonService } from "../../Services/lessonService";
import { moduleService } from "../../Services/moduleService";
import "./LectureDetail.css";

export default function LectureDetail() {
    const { courseId, lessonId, moduleId, lectureId } = useParams();
    const navigate = useNavigate();
    const [lectureTree, setLectureTree] = useState([]);
    const [currentLecture, setCurrentLecture] = useState(null);
    const [module, setModule] = useState(null);
    const [lesson, setLesson] = useState(null);
    const [course, setCourse] = useState(null);
    const [loadingTree, setLoadingTree] = useState(true);
    const [loadingLecture, setLoadingLecture] = useState(false);
    const [error, setError] = useState("");
    const moduleStartedRef = useRef(new Set());

    // Helper function to find first leaf lecture (lecture without children)
    const findFirstLeafLecture = (tree) => {
        for (const item of tree) {
            const children = item.children || item.Children || [];
            if (children.length === 0) {
                return item.lectureId || item.LectureId;
            }
            const childId = findFirstLeafLecture(children);
            if (childId) return childId;
        }
        return null;
    };

    // Fetch lecture detail by ID
    const fetchLectureDetail = useCallback(async (selectedLectureId) => {
        try {
            setLoadingLecture(true);
            setError("");

            const lectureResponse = await lectureService.getLectureById(selectedLectureId);
            if (lectureResponse.data?.success && lectureResponse.data?.data) {
                setCurrentLecture(lectureResponse.data.data);
            } else {
                setError(lectureResponse.data?.message || "Không thể tải nội dung bài giảng");
            }
        } catch (err) {
            console.error("Error fetching lecture detail:", err);
            setError("Không thể tải nội dung bài giảng");
        } finally {
            setLoadingLecture(false);
        }
    }, []);

    // Fetch lecture tree on mount or when moduleId changes
    useEffect(() => {
        const fetchLectureTree = async () => {
            try {
                setLoadingTree(true);
                setError("");

                // Gọi API hoàn thành module khi vào trang lecture - chỉ gọi một lần cho mỗi moduleId
                const parsedModuleId = typeof moduleId === 'string' ? parseInt(moduleId) : moduleId;
                if (parsedModuleId && !isNaN(parsedModuleId) && !moduleStartedRef.current.has(parsedModuleId)) {
                    try {
                        console.log(`Starting module ${parsedModuleId}...`);
                        const response = await moduleService.startModule(parsedModuleId);
                        moduleStartedRef.current.add(parsedModuleId);
                        console.log(`Module ${parsedModuleId} started successfully:`, response?.data);
                    } catch (err) {
                        console.error(`Error starting module ${parsedModuleId}:`, err);
                        console.error("Error details:", err.response?.data || err.message);
                        // Tiếp tục load dữ liệu dù API có lỗi
                    }
                } else {
                    if (moduleStartedRef.current.has(parsedModuleId)) {
                        console.log(`Module ${parsedModuleId} already started, skipping API call`);
                    } else {
                        console.warn(`Invalid moduleId: ${moduleId} (parsed: ${parsedModuleId})`);
                    }
                }

                // Fetch course info
                const courseResponse = await courseService.getCourseById(courseId);
                if (courseResponse.data?.success && courseResponse.data?.data) {
                    setCourse(courseResponse.data.data);
                }

                // Fetch lesson info
                const lessonResponse = await lessonService.getLessonById(lessonId);
                if (lessonResponse.data?.success && lessonResponse.data?.data) {
                    setLesson(lessonResponse.data.data);
                }

                // Fetch module info
                const moduleResponse = await moduleService.getModuleById(moduleId);
                if (moduleResponse.data?.success && moduleResponse.data?.data) {
                    setModule(moduleResponse.data.data);
                }

                const treeResponse = await lectureService.getLectureTreeByModuleId(moduleId);
                if (treeResponse.data?.success && treeResponse.data?.data) {
                    setLectureTree(treeResponse.data.data);
                    
                    // If lectureId is provided in URL, fetch that lecture
                    // Otherwise, find and load first leaf lecture
                    if (lectureId) {
                        await fetchLectureDetail(parseInt(lectureId));
                    } else {
                        const firstLeafId = findFirstLeafLecture(treeResponse.data.data);
                        if (firstLeafId) {
                            navigate(`/course/${courseId}/lesson/${lessonId}/module/${moduleId}/lecture/${firstLeafId}`, { replace: true });
                            await fetchLectureDetail(firstLeafId);
                        }
                    }
                } else {
                    setError(treeResponse.data?.message || "Không thể tải danh sách bài giảng");
                }
            } catch (err) {
                console.error("Error fetching lecture tree:", err);
                setError("Không thể tải dữ liệu bài giảng");
            } finally {
                setLoadingTree(false);
            }
        };

        if (moduleId) {
            fetchLectureTree();
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [moduleId, courseId, lessonId, lectureId]);

    // Fetch lecture detail when lectureId changes (but not during initial tree load)
    useEffect(() => {
        if (lectureId && !loadingTree) {
            fetchLectureDetail(parseInt(lectureId));
        }
    }, [lectureId, loadingTree, fetchLectureDetail]);

    // Handle lecture click from sidebar
    const handleLectureClick = useCallback((selectedLectureId) => {
        navigate(`/course/${courseId}/lesson/${lessonId}/module/${moduleId}/lecture/${selectedLectureId}`);
        // fetchLectureDetail will be called by useEffect when lectureId changes
    }, [courseId, lessonId, moduleId, navigate]);

    if (loadingTree) {
        return (
            <>
                <MainHeader />
                <div className="lecture-detail-container">
                    <div className="loading-message">Đang tải...</div>
                </div>
            </>
        );
    }

    if (error && !lectureTree.length) {
        return (
            <>
                <MainHeader />
                <div className="lecture-detail-container">
                    <div className="error-message">{error}</div>
                </div>
            </>
        );
    }

    const handleBackClick = () => {
        navigate(`/course/${courseId}/lesson/${lessonId}`);
    };

    const lessonTitle = lesson?.title || lesson?.Title || "Bài học";
    const courseTitle = course?.title || course?.Title || "Khóa học";
    const moduleName = module?.name || module?.Name || "Module";

    return (
        <>
            <MainHeader />
            <div className="lecture-detail-container">
                <Container className="lecture-breadcrumb-container">
                    <div className="lecture-detail-breadcrumb">
                        <span onClick={() => navigate("/my-courses")} className="breadcrumb-link">
                            Khóa học của tôi
                        </span>
                        <span className="breadcrumb-separator">/</span>
                        <span onClick={() => navigate(`/course/${courseId}`)} className="breadcrumb-link">
                            {courseTitle}
                        </span>
                        <span className="breadcrumb-separator">/</span>
                        <span onClick={() => navigate(`/course/${courseId}/learn`)} className="breadcrumb-link">
                            Lesson
                        </span>
                        <span className="breadcrumb-separator">/</span>
                        <span onClick={() => navigate(`/course/${courseId}/lesson/${lessonId}`)} className="breadcrumb-link">
                            {lessonTitle}
                        </span>
                        <span className="breadcrumb-separator">/</span>
                        <span className="breadcrumb-current">{moduleName}</span>
                    </div>
                </Container>
                <div className="lecture-detail-wrapper">
                    <div className="lecture-sidebar-col">
                        <LectureSidebar
                            lectureTree={lectureTree}
                            currentLectureId={currentLecture?.lectureId || currentLecture?.LectureId || (lectureId ? parseInt(lectureId) : null)}
                            onLectureClick={handleLectureClick}
                        />
                    </div>
                    <div className="lecture-content-col">
                        <LectureContent
                            lecture={currentLecture}
                            loading={loadingLecture}
                            error={error}
                            onBackClick={handleBackClick}
                        />
                    </div>
                </div>
            </div>
        </>
    );
}

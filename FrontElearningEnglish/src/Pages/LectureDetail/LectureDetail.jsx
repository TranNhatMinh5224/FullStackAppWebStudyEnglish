import React, { useState, useEffect, useCallback, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import MainHeader from "../../Components/Header/MainHeader";
import LectureTree from "../../Components/LectureDetail/LectureTree/LectureTree";
import LectureHeader from "../../Components/LectureDetail/LectureHeader/LectureHeader";
import LectureContent from "../../Components/LectureDetail/LectureContent/LectureContent";
import LectureFooter from "../../Components/LectureDetail/LectureFooter/LectureFooter";
import { lectureService } from "../../Services/lectureService";
import { courseService } from "../../Services/courseService";
import { lessonService } from "../../Services/lessonService";
import { moduleService } from "../../Services/moduleService";
import "./LectureDetail.css";

export default function LectureDetail() {
    const { courseId, lessonId, moduleId, lectureId } = useParams();
    const navigate = useNavigate();
    
    // State
    const [lectureTree, setLectureTree] = useState([]);
    const [currentLecture, setCurrentLecture] = useState(null);
    const [selectedLectureId, setSelectedLectureId] = useState(null); // Track selected lecture ID immediately
    const [module, setModule] = useState(null);
    const [lesson, setLesson] = useState(null);
    const [course, setCourse] = useState(null);
    const [loadingTree, setLoadingTree] = useState(true);
    const [loadingLecture, setLoadingLecture] = useState(false);
    const [error, setError] = useState("");
    const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
    
    // Refs
    const moduleStartedRef = useRef(new Set());

    // Helper: Flatten lecture tree to get all leaf lectures
    const flattenLectureTree = useCallback((tree, result = []) => {
        for (const item of tree) {
            const children = item.children || item.Children || [];
            if (children.length === 0) {
                result.push(item);
            } else {
                flattenLectureTree(children, result);
            }
        }
        return result;
    }, []);

    // Helper: Find first leaf lecture
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

    // Get all leaf lectures for navigation
    const getAllLeafLectures = useCallback(() => {
        return flattenLectureTree(lectureTree);
    }, [lectureTree, flattenLectureTree]);

    // Get previous and next lecture
    const getNavigationLectures = useCallback(() => {
        const allLectures = getAllLeafLectures();
        const currentId = currentLecture?.lectureId || currentLecture?.LectureId || (lectureId ? parseInt(lectureId) : null);
        
        if (!currentId || allLectures.length === 0) {
            return { previous: null, next: null };
        }

        const currentIndex = allLectures.findIndex(
            (lecture) => (lecture.lectureId || lecture.LectureId) === currentId
        );

        if (currentIndex === -1) {
            return { previous: null, next: null };
        }

        return {
            previous: currentIndex > 0 ? allLectures[currentIndex - 1] : null,
            next: currentIndex < allLectures.length - 1 ? allLectures[currentIndex + 1] : null,
        };
    }, [getAllLeafLectures, currentLecture, lectureId]);

    // Fetch lecture detail by ID
    const fetchLectureDetail = useCallback(async (lectureIdToLoad, updateUrl = true) => {
        try {
            // Update selected ID immediately for instant UI feedback
            setSelectedLectureId(lectureIdToLoad);
            setLoadingLecture(true);
            setError("");

            const lectureResponse = await lectureService.getLectureById(lectureIdToLoad);
            if (lectureResponse.data?.success && lectureResponse.data?.data) {
                setCurrentLecture(lectureResponse.data.data);
                
                // Update URL after content is loaded (smooth UX, no reload)
                if (updateUrl) {
                    const newUrl = `/course/${courseId}/lesson/${lessonId}/module/${moduleId}/lecture/${lectureIdToLoad}`;
                    // Use replace to avoid creating new history entry and prevent reload
                    window.history.replaceState(null, '', newUrl);
                }
                
                // Scroll to top smoothly when new content loads
                const contentSection = document.querySelector('.lecture-content-section');
                if (contentSection) {
                    contentSection.scrollTo({ top: 0, behavior: 'smooth' });
                }
            } else {
                setError(lectureResponse.data?.message || "Không thể tải nội dung bài giảng");
                setSelectedLectureId(null); // Reset on error
            }
        } catch (err) {
            console.error("Error fetching lecture detail:", err);
            setError("Không thể tải nội dung bài giảng");
            setSelectedLectureId(null); // Reset on error
        } finally {
            setLoadingLecture(false);
        }
    }, [courseId, lessonId, moduleId]);

    // Fetch lecture tree and related data
    useEffect(() => {
        const fetchLectureTree = async () => {
            try {
                setLoadingTree(true);
                setError("");

                // Start module (only once per moduleId)
                const parsedModuleId = typeof moduleId === 'string' ? parseInt(moduleId) : moduleId;
                if (parsedModuleId && !isNaN(parsedModuleId) && !moduleStartedRef.current.has(parsedModuleId)) {
                    try {
                        await moduleService.startModule(parsedModuleId);
                        moduleStartedRef.current.add(parsedModuleId);
                    } catch (err) {
                        console.error("Error starting module:", err);
                    }
                }

                // Fetch course, lesson, module, tree in parallel
                const [courseResponse, lessonResponse, moduleResponse, treeResponse] = await Promise.all([
                    courseService.getCourseById(courseId),
                    lessonService.getLessonById(lessonId),
                    moduleService.getModuleById(moduleId),
                    lectureService.getLectureTreeByModuleId(moduleId)
                ]);

                if (courseResponse.data?.success && courseResponse.data?.data) {
                    setCourse(courseResponse.data.data);
                }

                if (lessonResponse.data?.success && lessonResponse.data?.data) {
                    setLesson(lessonResponse.data.data);
                }

                if (moduleResponse.data?.success && moduleResponse.data?.data) {
                    setModule(moduleResponse.data.data);
                }

                if (treeResponse.data?.success && treeResponse.data?.data) {
                    setLectureTree(treeResponse.data.data);
                    
                    // Load lecture if provided, otherwise load first leaf
                    if (lectureId) {
                        const parsedId = parseInt(lectureId);
                        setSelectedLectureId(parsedId); // Set immediately for UI feedback
                        await fetchLectureDetail(parsedId, false); // Don't update URL on initial load
                    } else {
                        const firstLeafId = findFirstLeafLecture(treeResponse.data.data);
                        if (firstLeafId) {
                            setSelectedLectureId(firstLeafId); // Set immediately for UI feedback
                            await fetchLectureDetail(firstLeafId, true); // Update URL after loading
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
    }, [moduleId, courseId, lessonId, navigate, fetchLectureDetail]);

    // Fetch lecture detail when lectureId changes (e.g., browser back/forward)
    useEffect(() => {
        if (lectureId && !loadingTree && lectureTree.length > 0) {
            const parsedId = parseInt(lectureId);
            const currentId = currentLecture?.lectureId || currentLecture?.LectureId;
            // Only fetch if it's a different lecture (avoid duplicate calls)
            if (parsedId !== currentId && parsedId !== selectedLectureId) {
                setSelectedLectureId(parsedId); // Set immediately for UI feedback
                fetchLectureDetail(parsedId, false); // Don't update URL, it's already updated by browser
            }
        }
    }, [lectureId, loadingTree, lectureTree.length, currentLecture, selectedLectureId, fetchLectureDetail]);

    // Handle lecture click from tree - Load content first, then update URL (smooth UX)
    const handleLectureClick = useCallback((selectedLectureId) => {
        // Load content immediately without navigation (no reload)
        fetchLectureDetail(selectedLectureId, true);
    }, [fetchLectureDetail]);

    // Handle navigation - Load content first, then update URL (smooth UX)
    const handlePrevious = useCallback(() => {
        const { previous } = getNavigationLectures();
        if (previous) {
            const prevId = previous.lectureId || previous.LectureId;
            fetchLectureDetail(prevId, true);
        }
    }, [getNavigationLectures, fetchLectureDetail]);

    const handleNext = useCallback(() => {
        const { next } = getNavigationLectures();
        if (next) {
            const nextId = next.lectureId || next.LectureId;
            fetchLectureDetail(nextId, true);
        }
    }, [getNavigationLectures, fetchLectureDetail]);

    // Loading state
    if (loadingTree) {
        return (
            <>
                <MainHeader />
                <div className="lecture-detail-page">
                    <div className="lecture-loading">
                        <div className="loading-spinner"></div>
                        <p>Đang tải...</p>
                    </div>
                </div>
            </>
        );
    }

    // Error state
    if (error && !lectureTree.length) {
        return (
            <>
                <MainHeader />
                <div className="lecture-detail-page">
                    <div className="lecture-error">
                        <p>{error}</p>
                    </div>
                </div>
            </>
        );
    }

    const lessonTitle = lesson?.title || lesson?.Title || "Bài học";
    const courseTitle = course?.title || course?.Title || "Khóa học";
    const moduleName = module?.name || module?.Name || "Module";
    const navigation = getNavigationLectures();
    // Use selectedLectureId for immediate UI feedback, fallback to currentLecture or URL param
    const currentLectureId = selectedLectureId || currentLecture?.lectureId || currentLecture?.LectureId || (lectureId ? parseInt(lectureId) : null);

    return (
        <>
            <MainHeader />
            <div className={`lecture-detail-page d-flex flex-column ${sidebarCollapsed ? 'sidebar-collapsed' : ''}`}>
                {/* Header - Full Width */}
                <LectureHeader
                    sidebarCollapsed={sidebarCollapsed}
                    onToggleSidebar={() => setSidebarCollapsed(!sidebarCollapsed)}
                    courseId={courseId}
                    lessonId={lessonId}
                    courseTitle={courseTitle}
                    lessonTitle={lessonTitle}
                    moduleName={moduleName}
                />

                {/* Content Area - Split Layout */}
                <div className="lecture-content-wrapper d-flex flex-grow-1 position-relative">
                    {/* Mobile Overlay */}
                    {!sidebarCollapsed && (
                        <div 
                            className="lecture-sidebar-overlay d-md-none"
                            onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
                        />
                    )}

                    {/* Left: Tree */}
                    <aside className={`lecture-sidebar d-flex flex-column ${sidebarCollapsed ? 'collapsed' : ''}`}>
                        <LectureTree
                            lectureTree={lectureTree}
                            currentLectureId={currentLectureId}
                            onLectureClick={handleLectureClick}
                        />
                    </aside>

                    {/* Right: Content & Footer */}
                    <div className="lecture-main-wrapper d-flex flex-column flex-grow-1">
                        {/* Content Section */}
                        <section className="lecture-content-section flex-grow-1">
                            <div className="lecture-content-container">
                                <LectureContent
                                    lecture={currentLecture}
                                    loading={loadingLecture}
                                    error={error}
                                />
                            </div>
                        </section>

                        {/* Footer */}
                        {currentLecture && (
                            <LectureFooter
                                onPrevious={handlePrevious}
                                onNext={handleNext}
                                hasPrevious={!!navigation.previous}
                                hasNext={!!navigation.next}
                            />
                        )}
                    </div>
                </div>
            </div>
        </>
    );
}

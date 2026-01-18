import React, { useRef, useEffect } from "react";
import LectureHeader from "../LectureHeader/LectureHeader";
import LectureContent from "../LectureContent/LectureContent";
import LectureFooter from "../LectureFooter/LectureFooter";
import "./LectureMain.css";

const LectureMain = ({
    sidebarCollapsed,
    onToggleSidebar,
    courseId,
    lessonId,
    courseTitle,
    lessonTitle,
    moduleName,
    currentLecture,
    loadingLecture,
    error,
    onPrevious,
    onNext,
    hasPrevious,
    hasNext
}) => {
    const contentWrapperRef = useRef(null);

    // Scroll to top when lecture changes
    useEffect(() => {
        if (currentLecture && contentWrapperRef.current) {
            contentWrapperRef.current.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }, [currentLecture]);

    return (
        <main className="lecture-main">
            {/* Header */}
            <LectureHeader
                sidebarCollapsed={sidebarCollapsed}
                onToggleSidebar={onToggleSidebar}
                courseId={courseId}
                lessonId={lessonId}
                courseTitle={courseTitle}
                lessonTitle={lessonTitle}
                moduleName={moduleName}
            />

            {/* Content */}
            <section className="lecture-content-section" ref={contentWrapperRef}>
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
                    onPrevious={onPrevious}
                    onNext={onNext}
                    hasPrevious={hasPrevious}
                    hasNext={hasNext}
                />
            )}
        </main>
    );
};

LectureMain.displayName = "LectureMain";

export default LectureMain;

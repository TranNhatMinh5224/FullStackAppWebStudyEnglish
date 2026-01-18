import React from "react";
import { FaChevronLeft, FaChevronRight } from "react-icons/fa";
import "./LectureFooter.css";

const LectureFooter = ({ onPrevious, onNext, hasPrevious, hasNext }) => {
    return (
        <footer className="lecture-footer">
            <button
                className="nav-btn nav-btn-prev"
                onClick={onPrevious}
                disabled={!hasPrevious}
            >
                <FaChevronLeft />
                <span>Mục trước</span>
            </button>
            <button
                className="nav-btn nav-btn-next"
                onClick={onNext}
                disabled={!hasNext}
            >
                <span>Mục tiếp</span>
                <FaChevronRight />
            </button>
        </footer>
    );
};

LectureFooter.displayName = "LectureFooter";

export default LectureFooter;

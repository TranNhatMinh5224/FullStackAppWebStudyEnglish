import React from "react";
import { FaChevronLeft, FaChevronRight } from "react-icons/fa";
import "./FlashCardNavigation.css";

export default function FlashCardNavigation({ onPrevious, onNext, canGoPrevious, canGoNext }) {
    return (
        <div className="flashcard-navigation">
            <button
                className={`nav-button prev-button ${!canGoPrevious ? "disabled" : ""}`}
                onClick={onPrevious}
                disabled={!canGoPrevious}
            >
                <FaChevronLeft />
            </button>
            <button
                className={`nav-button next-button ${!canGoNext ? "disabled" : ""}`}
                onClick={onNext}
                disabled={!canGoNext}
            >
                <FaChevronRight />
            </button>
        </div>
    );
}


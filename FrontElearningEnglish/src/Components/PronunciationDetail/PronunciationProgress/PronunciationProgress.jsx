import React from "react";
import "./PronunciationProgress.css";

export default function PronunciationProgress({ score, showScore, feedback }) {
    // Normalize score: ensure it's a number between 0 and 100
    const scoreNum = typeof score === 'number' ? score : parseFloat(score) || 0;
    const normalizedScore = Math.min(Math.max(scoreNum, 0), 100);
    const circumference = 2 * Math.PI * 60; // radius = 60
    const offset = circumference - (normalizedScore / 100) * circumference;

    // Show score if showScore is true (even if score is 0, because 0 is a valid assessment result)
    const displayScore = showScore;

    return (
        <div className="pronunciation-progress-container">
            <div className="progress-circle-wrapper">
                <svg className="progress-circle" width="140" height="140">
                    <circle
                        className="progress-circle-bg"
                        cx="70"
                        cy="70"
                        r="60"
                        fill="none"
                        stroke="#e5e7eb"
                        strokeWidth="8"
                    />
                    {displayScore && (
                        <circle
                            className="progress-circle-fill"
                            cx="70"
                            cy="70"
                            r="60"
                            fill="none"
                            stroke="#41d6e3"
                            strokeWidth="8"
                            strokeDasharray={circumference}
                            strokeDashoffset={offset}
                            strokeLinecap="round"
                            transform="rotate(-90 70 70)"
                        />
                    )}
                </svg>
                <div className="progress-score">
                    <span className="score-number">
                        {displayScore ? Math.round(normalizedScore) : 0}
                    </span>
                </div>
            </div>
            {feedback && (
                <div className="progress-feedback-wrapper">
                    <span className="score-feedback">{feedback}</span>
                </div>
            )}
        </div>
    );
}



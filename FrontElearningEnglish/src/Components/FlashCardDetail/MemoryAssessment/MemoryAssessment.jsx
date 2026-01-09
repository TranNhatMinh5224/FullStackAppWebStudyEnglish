import React, { useState } from "react";
import "./MemoryAssessment.css";

export default function MemoryAssessment({ onAssessment }) {
    const [selectedLevel, setSelectedLevel] = useState(null);

    const memoryLevels = [
        { id: 0, label: "Không nhớ", color: "#f3f4f6" },
        { id: 1, label: "Hơi nhớ", color: "#f3f4f6" },
        { id: 2, label: "Nhớ", color: "#41d6e3" },
        { id: 3, label: "Khá nhớ", color: "#f3f4f6" },
        { id: 4, label: "Đã thuộc", color: "#f3f4f6" },
    ];

    const handleLevelClick = (level) => {
        setSelectedLevel(level.id);
        if (onAssessment) {
            onAssessment(level.id);
        }
        // Reset selection after a short delay
        setTimeout(() => {
            setSelectedLevel(null);
        }, 300);
    };

    return (
        <div className="memory-assessment">
            <p className="memory-assessment-question">Bạn nhớ từ này ở mức độ nào?</p>
            <div className="memory-assessment-buttons">
                {memoryLevels.map((level) => (
                    <button
                        key={level.id}
                        className={`memory-level-btn ${selectedLevel === level.id ? "selected" : ""}`}
                        style={{
                            backgroundColor: selectedLevel === level.id ? level.color : "#ffffff",
                            borderColor: level.color === "#41d6e3" && selectedLevel !== level.id ? level.color : "#e5e7eb",
                        }}
                        onClick={() => handleLevelClick(level)}
                    >
                        {level.label}
                    </button>
                ))}
            </div>
        </div>
    );
}


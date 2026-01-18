import React from "react";
import "./MemoryAssessment.css";

export default function MemoryAssessment({ selectedQuality, onQualitySelect }) {
    const qualityOptions = [
        { value: 1, label: "Không nhớ", color: "#ef4444" },
        { value: 2, label: "Hơi nhớ", color: "#f59e0b" },
        { value: 3, label: "Nhớ", color: "#3b82f6" },
        { value: 4, label: "Khá nhớ", color: "#10b981" },
        { value: 5, label: "Đã thuộc", color: "#8b5cf6" },
    ];

    return (
        <div className="memory-assessment-container d-flex flex-column align-items-center">
            <p className="memory-assessment-question text-center mb-3">
                Bạn nhớ từ này ở mức độ nào?
            </p>
            <div className="memory-assessment-buttons d-flex flex-wrap justify-content-center gap-2 w-100">
                {qualityOptions.map((option) => (
                    <button
                        key={option.value}
                        className={`memory-assessment-btn btn ${
                            selectedQuality === option.value ? "selected" : ""
                        }`}
                        style={{
                            backgroundColor:
                                selectedQuality === option.value
                                    ? option.color
                                    : "#ffffff",
                            color:
                                selectedQuality === option.value
                                    ? "#ffffff"
                                    : "#1f2937",
                            borderColor: option.color,
                        }}
                        onClick={() => onQualitySelect(option.value)}
                    >
                        <span className="small">{option.label}</span>
                    </button>
                ))}
            </div>
        </div>
    );
}


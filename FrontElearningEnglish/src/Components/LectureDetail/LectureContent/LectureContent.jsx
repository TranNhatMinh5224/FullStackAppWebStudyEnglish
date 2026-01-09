import React from "react";
import { Container } from "react-bootstrap";
import { FaArrowLeft } from "react-icons/fa";
import MarkdownViewer from "../MarkdownViewer/MarkdownViewer";
import "./LectureContent.css";

export default function LectureContent({ lecture, loading, error, onBackClick }) {
    if (loading) {
        return (
            <div className="lecture-content-wrapper">
                <Container className="lecture-content-container">
                    <div className="loading-message">Đang tải nội dung...</div>
                </Container>
            </div>
        );
    }

    if (error) {
        return (
            <div className="lecture-content-wrapper">
                <Container className="lecture-content-container">
                    <div className="error-message">{error}</div>
                </Container>
            </div>
        );
    }

    if (!lecture) {
        return (
            <div className="lecture-content-wrapper">
                <Container className="lecture-content-container">
                    <div className="no-lecture-message">
                        <p>Chọn một bài giảng từ danh sách bên trái để xem nội dung</p>
                    </div>
                </Container>
            </div>
        );
    }

    return (
        <div className="lecture-content-wrapper">
            
            <Container className="lecture-content-container">
                <MarkdownViewer lecture={lecture} />
            </Container>
        </div>
    );
}


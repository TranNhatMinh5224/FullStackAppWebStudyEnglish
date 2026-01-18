import React from "react";
import { FaCheckCircle } from "react-icons/fa";
import { Card, Row, Col } from "react-bootstrap";
import "./QuizNavigation.css";

export default function QuizNavigation({ questions, currentIndex, answers, onGoToQuestion }) {
    const getQuestionStatus = (question, index) => {
        const questionId = question.questionId || question.QuestionId;
        const hasAnswer = answers[questionId] !== undefined && answers[questionId] !== null;
        const isCurrent = index === currentIndex;
        
        if (isCurrent) return "current";
        if (hasAnswer) return "answered";
        return "unanswered";
    };

    const countAnswered = (answersObj) => {
        if (!answersObj) return 0;
        try {
            return Object.values(answersObj).filter(v => {
                if (v === null || v === undefined) return false;
                if (Array.isArray(v)) return v.length > 0;
                if (typeof v === 'object') return Object.keys(v).length > 0;
                if (typeof v === 'string') return v.trim() !== "";
                return true; // number/boolean treated as answered
            }).length;
        } catch (e) {
            return 0;
        }
    };

    const answeredCount = countAnswered(answers);

    return (
        <Card className="quiz-navigation">
            <Card.Body>
                <div className="navigation-header d-flex justify-content-between align-items-center">
                    <h4 className="navigation-title">Danh sách câu hỏi</h4>
                    <div className="navigation-stats d-flex align-items-center">
                        <div className="stat-panel d-flex align-items-center justify-content-center">
                                <div className="stat-content d-flex align-items-center">
                                    <FaCheckCircle className="stat-icon" />
                                    <span className="stat-text">{answeredCount}/{questions ? questions.length : 0}</span>
                                </div>
                        </div>
                    </div>
                </div>
                <Row className="navigation-grid g-2 mb-3">
                    {questions.map((question, index) => {
                        const status = getQuestionStatus(question, index);
                        const questionId = question.questionId || question.QuestionId;
                        return (
                            <Col key={questionId || index} xs={4} sm={3} md={2} lg={2} xl={2}>
                                <button
                                    type="button"
                                    className={`navigation-item ${status} w-100 d-flex align-items-center justify-content-center`}
                                    onClick={() => onGoToQuestion(index)}
                                >
                                    {index + 1}
                                </button>
                            </Col>
                        );
                    })}
                </Row>
                <div className="navigation-legend d-flex flex-column">
                    <div className="legend-item d-flex align-items-center">
                        <div className="legend-color current"></div>
                        <span>Đang làm</span>
                    </div>
                    <div className="legend-item d-flex align-items-center">
                        <div className="legend-color answered"></div>
                        <span>Đã trả lời</span>
                    </div>
                    <div className="legend-item d-flex align-items-center">
                        <div className="legend-color unanswered"></div>
                        <span>Chưa trả lời</span>
                    </div>
                </div>
            </Card.Body>
        </Card>
    );
}


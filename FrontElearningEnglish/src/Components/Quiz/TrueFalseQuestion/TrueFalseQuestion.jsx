
import React from "react";
import { Row, Col, Card, Badge } from "react-bootstrap";
import { FaCheckCircle } from "react-icons/fa";
import "./TrueFalseQuestion.css";

export default function TrueFalseQuestion({ question, answer, onChange }) {
    const options = question.options || question.Options || [];
    
    // Find True and False options
    const trueOption = options.find(opt => {
        const text = (opt.optionText || opt.OptionText || opt.text || opt.Text || "").toLowerCase();
        return text.includes("true") || text.includes("đúng");
    }) || options[0]; // Fallback to first option
    
    const falseOption = options.find(opt => {
        const text = (opt.optionText || opt.OptionText || opt.text || opt.Text || "").toLowerCase();
        return text.includes("false") || text.includes("sai");
    }) || options[1]; // Fallback to second option

    const trueOptionId = trueOption ? (trueOption.optionId || trueOption.OptionId || trueOption.answerOptionId || trueOption.AnswerOptionId) : null;
    const falseOptionId = falseOption ? (falseOption.optionId || falseOption.OptionId || falseOption.answerOptionId || falseOption.AnswerOptionId) : null;
    
    const trueText = trueOption ? (trueOption.optionText || trueOption.OptionText || trueOption.text || trueOption.Text || "Đúng") : "Đúng";
    const falseText = falseOption ? (falseOption.optionText || falseOption.OptionText || falseOption.text || falseOption.Text || "Sai") : "Sai";

    const handleChange = (optionId) => {
        onChange(optionId);
    };

    return (
        <div className="true-false-question">
            <Row className="true-false-options">
                {trueOptionId && (
                    <Col>
                        <Card
                            className={`true-false-option${answer === trueOptionId ? " selected" : ""}`}
                            onClick={() => handleChange(trueOptionId)}
                            style={{ cursor: "pointer" }}
                        >
                            <Card.Body className="d-flex align-items-center justify-content-between p-4">
                                <div className="d-flex align-items-center flex-grow-1 justify-content-center">
                                    <div className={`option-radio me-3 ${answer === trueOptionId ? "selected" : ""}`}>
                                        {answer === trueOptionId && <FaCheckCircle className="check-icon" />}
                                    </div>
                                    <span className="option-label fw-bold">{trueText}</span>
                                </div>
                                {answer === trueOptionId && (
                                    <Badge bg="success" className="selected-badge">
                                        <FaCheckCircle className="me-1" />
                                        Đã chọn
                                    </Badge>
                                )}
                            </Card.Body>
                        </Card>
                    </Col>
                )}
                {falseOptionId && (
                    <Col>
                        <Card
                            className={`true-false-option${answer === falseOptionId ? " selected" : ""}`}
                            onClick={() => handleChange(falseOptionId)}
                            style={{ cursor: "pointer" }}
                        >
                            <Card.Body className="d-flex align-items-center justify-content-between p-4">
                                <div className="d-flex align-items-center flex-grow-1 justify-content-center">
                                    <div className={`option-radio me-3 ${answer === falseOptionId ? "selected" : ""}`}>
                                        {answer === falseOptionId && <FaCheckCircle className="check-icon" />}
                                    </div>
                                    <span className="option-label fw-bold">{falseText}</span>
                                </div>
                                {answer === falseOptionId && (
                                    <Badge bg="success" className="selected-badge">
                                        <FaCheckCircle className="me-1" />
                                        Đã chọn
                                    </Badge>
                                )}
                            </Card.Body>
                        </Card>
                    </Col>
                )}
            </Row>
        </div>
    );
}


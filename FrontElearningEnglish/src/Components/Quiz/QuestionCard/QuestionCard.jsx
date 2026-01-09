import React from "react";
import MultipleChoiceQuestion from "../MultipleChoiceQuestion/MultipleChoiceQuestion";
import MatchingQuestion from "../MatchingQuestion/MatchingQuestion";
import OrderingQuestion from "../OrderingQuestion/OrderingQuestion";
import FillBlankQuestion from "../FillBlankQuestion/FillBlankQuestion";
import TrueFalseQuestion from "../TrueFalseQuestion/TrueFalseQuestion";
import { Card, Row, Col, Badge } from "react-bootstrap";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import "./QuestionCard.css";

export default function QuestionCard({ question, answer, onChange, questionNumber, totalQuestions }) {
    // Handle both camelCase and PascalCase
    const questionType = question?.type !== undefined ? question.type : (question?.Type !== undefined ? question.Type : 0);

    const renderQuestion = () => {
        if (!question) return null;

        switch (questionType) {
            case 1: // MultipleChoice
                return (
                    <MultipleChoiceQuestion
                        question={question}
                        answer={answer}
                        onChange={onChange}
                    />
                );
            case 2: // MultipleAnswers
                return (
                    <MultipleChoiceQuestion
                        question={question}
                        answer={answer}
                        onChange={onChange}
                        multiple={true}
                    />
                );
            case 3: // TrueFalse
                return (
                    <TrueFalseQuestion
                        question={question}
                        answer={answer}
                        onChange={onChange}
                    />
                );
            case 4: // FillBlank
                return (
                    <FillBlankQuestion
                        question={question}
                        answer={answer}
                        onChange={onChange}
                    />
                );
            case 5: // Matching
                return (
                    <MatchingQuestion
                        question={question}
                        answer={answer}
                        onChange={onChange}
                    />
                );
            case 6: // Ordering
                return (
                    <OrderingQuestion
                        question={question}
                        answer={answer}
                        onChange={onChange}
                    />
                );
            default:
                return (
                    <MultipleChoiceQuestion
                        question={question}
                        answer={answer}
                        onChange={onChange}
                    />
                );
        }
    };

    if (!question) {
        return (
            <Card className="question-card">
                <Card.Body>
                    <div className="no-question-message">Không có câu hỏi</div>
                </Card.Body>
            </Card>
        );
    }

    // Extract group info if available
    const groupInfo = question._groupInfo;

    return (
        <Card className="question-card">
            <Card.Body>
                <Row className="question-header align-items-center">
                    <Col xs="auto" className="question-number">
                        Câu {questionNumber}/{totalQuestions}
                    </Col>
                    <Col xs="auto" className="question-points">
                        <Badge bg="info" className="px-3 py-2">
                            {question.points || question.Points || 0} điểm
                        </Badge>
                    </Col>
                </Row>
                
                {/* Display Group Information if available */}
                {groupInfo && (groupInfo.groupName || groupInfo.groupTitle || groupInfo.groupImgUrl || groupInfo.groupVideoUrl || groupInfo.groupDescription) && (
                    <div className="question-group-info mb-3 p-3 bg-light rounded border">
                        {/* Group Header: Name + Title */}
                        {(groupInfo.groupName || groupInfo.groupTitle) && (
                            <div className="group-header mb-2">
                                {groupInfo.groupName && (
                                    <span className="group-name badge bg-primary me-2">{groupInfo.groupName}</span>
                                )}
                                {groupInfo.groupTitle && (
                                    <span className="group-title fw-semibold">{groupInfo.groupTitle}</span>
                                )}
                            </div>
                        )}
                        
                        {/* Group Description (Reading passage) */}
                        {groupInfo.groupDescription && (
                            <div className="group-description mb-3 p-3 bg-white border rounded" style={{ whiteSpace: 'pre-wrap' }}>
                                {groupInfo.groupDescription}
                            </div>
                        )}
                        
                        {/* Group Image */}
                        {groupInfo.groupImgUrl && (
                            <div className="group-media mb-2">
                                <img 
                                    src={groupInfo.groupImgUrl} 
                                    alt="Group context" 
                                    className="media-element group-img" 
                                    style={{ maxWidth: '100%', height: 'auto', borderRadius: '8px' }}
                                />
                            </div>
                        )}
                        
                        {/* Group Video/Audio - detect type from groupVideoType */}
                        {groupInfo.groupVideoUrl && (
                            <div className="group-media mb-2">
                                {(groupInfo.groupVideoType?.startsWith('audio') || 
                                  groupInfo.groupVideoUrl?.includes('.mp3') || 
                                  groupInfo.groupVideoUrl?.includes('.wav')) ? (
                                    <audio 
                                        src={groupInfo.groupVideoUrl} 
                                        controls 
                                        className="media-element group-audio w-100"
                                    />
                                ) : (
                                    <video 
                                        src={groupInfo.groupVideoUrl} 
                                        controls 
                                        className="media-element group-video"
                                        style={{ maxWidth: '100%', height: 'auto', borderRadius: '8px' }}
                                    />
                                )}
                                {/* Show video duration if available */}
                                {groupInfo.groupVideoDuration && (
                                    <small className="text-muted d-block mt-1">
                                        Thời lượng: {Math.floor(groupInfo.groupVideoDuration / 60)}:{String(groupInfo.groupVideoDuration % 60).padStart(2, '0')}
                                    </small>
                                )}
                            </div>
                        )}
                    </div>
                )}
                
                <div className="question-content">
                    {questionType !== 4 && (
                        <div className="question-text">
                            <ReactMarkdown remarkPlugins={[remarkGfm]}>
                                {question.questionText || question.QuestionText || question.stemText || question.StemText || "Câu hỏi"}
                            </ReactMarkdown>
                        </div>
                    )}
                    {(question.mediaUrl || question.MediaUrl) && (
                        <div className="question-media">
                            {(() => {
                                const mediaUrl = question.mediaUrl || question.MediaUrl;
                                if (mediaUrl.includes('.mp4') || mediaUrl.includes('.webm')) {
                                    return <video src={mediaUrl} controls className="media-element" />;
                                } else if (mediaUrl.includes('.mp3') || mediaUrl.includes('.wav')) {
                                    return <audio src={mediaUrl} controls className="media-element" />;
                                } else {
                                    return <img src={mediaUrl} alt="Question media" className="media-element" />;
                                }
                            })()}
                        </div>
                    )}
                    <div className="question-answer-section">
                        {renderQuestion()}
                    </div>
                </div>
            </Card.Body>
        </Card>
    );
}


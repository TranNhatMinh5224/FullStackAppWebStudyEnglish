import React from "react";
import { Row, Col } from "react-bootstrap";
import "./WelcomeSection.css";

export default function WelcomeSection({ displayName }) {
    return (
        <Row className="welcome-section g-3 g-md-4 align-items-center mb-4">
            <Col xs={12} className="welcome-section__left d-flex flex-column justify-content-center align-items-start">
                <h1>Chào mừng trở lại, {displayName}</h1>
                <p>Hãy tiếp tục hành trình học tiếng Anh nào.</p>
            </Col>
        </Row>
    );
}


import React from "react";
import { Container, Row, Col } from "react-bootstrap";
import "./CourseInfo.css";
import CourseDescription from "../CourseDescription/CourseDescription";

export default function CourseInfo({ course }) {
    return (
        <div className="course-info">
            <Container fluid>
                <Row>
                    <Col>
                        <section className="course-info-section d-flex flex-column">
                            <h2 className="course-info-title">Giới thiệu khoá học</h2>
                            
                            <div className="course-info-subsection d-flex flex-column">
                                <CourseDescription description={course.description} />
                            </div>
                        </section>
                    </Col>
                </Row>
            </Container>
        </div>
    );
}


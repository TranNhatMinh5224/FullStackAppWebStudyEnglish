import React from "react";
import { Row, Col, Container } from "react-bootstrap";
import "./Footer.css";

const FACEBOOK_GROUP_URL = "https://web.facebook.com/groups/843825855021989";

export default function Footer() {
  return (
    <footer className="footer">
      <Container>
        <Row className="footer-content g-4 g-md-5 mb-4">
          <Col xs={6} md={3} className="footer-section d-flex flex-column">
            <h4 className="footer-title">Catalunya English</h4>
            <ul className="footer-links d-flex flex-column">
              <li><a href="#about">Giới thiệu</a></li>
              <li><a href="#how-it-works">Cách hoạt động</a></li>
              <li><a href={FACEBOOK_GROUP_URL} target="_blank" rel="noopener noreferrer">Hỗ trợ</a></li>
              <li><a href="#download">Tải ứng dụng</a></li>
            </ul>
          </Col>

          <Col xs={6} md={3} className="footer-section d-flex flex-column">
            <h4 className="footer-title">Khóa học</h4>
            <ul className="footer-links d-flex flex-column">
              <li><a href="#vocabulary">Từ vựng tiếng Anh</a></li>
              <li><a href="#ielts">Luyện thi IELTS</a></li>
              <li><a href="#listening">Luyện nghe</a></li>
              <li><a href="#toeic">Luyện thi TOEIC</a></li>
            </ul>
          </Col>

          <Col xs={6} md={3} className="footer-section d-flex flex-column">
            <h4 className="footer-title">Tài liệu</h4>
            <ul className="footer-links d-flex flex-column">
              <li><a href="#vocab-basic">1000 Từ vựng cơ bản</a></li>
              <li><a href="#vocab-ielts">1200 Từ vựng IELTS</a></li>
              <li><a href="#vocab-toeic">1000 Từ vựng TOEIC</a></li>
              <li><a href="#phrasal-verbs">Cụm động từ</a></li>
            </ul>
          </Col>

          <Col xs={6} md={3} className="footer-section d-flex flex-column">
            <h4 className="footer-title">Liên hệ</h4>
            <ul className="footer-links d-flex flex-column">
              <li>
                <a 
                  href={FACEBOOK_GROUP_URL} 
                  target="_blank" 
                  rel="noopener noreferrer"
                >
                  Facebook Group
                </a>
              </li>
              <li><a href="mailto:support@catalunyaenglish.com">support@catalunyaenglish.com</a></li>
            </ul>
          </Col>
        </Row>

        <div className="footer-bottom d-flex justify-content-between align-items-center flex-wrap gap-3 pt-4 border-top">
          <p className="mb-0">&copy; 2025 Catalunya English. All rights reserved.</p>
          <div className="footer-legal d-flex align-items-center">
            <a href="#privacy">Privacy Policy</a>
            <span>|</span>
            <a href="#terms">Terms of Service</a>
          </div>
        </div>
      </Container>
    </footer>
  );
}

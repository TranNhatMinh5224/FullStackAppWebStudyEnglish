import React from "react";
import "./WelcomeFooter.css";

const FACEBOOK_GROUP_URL = "https://web.facebook.com/groups/843825855021989";

export default function WelcomeFooter() {
  return (
    <footer className="welcome-footer">
      <div className="footer-content">
        <div className="footer-section">
          <h4 className="footer-title">Catalunya English</h4>
          <ul className="footer-links">
            <li><a href="#about">Giới thiệu</a></li>
            <li><a href="#how-it-works">Cách hoạt động</a></li>
            <li><a href={FACEBOOK_GROUP_URL} target="_blank" rel="noopener noreferrer">Hỗ trợ</a></li>
            <li><a href="#download">Tải ứng dụng</a></li>
          </ul>
        </div>

        <div className="footer-section">
          <h4 className="footer-title">Khóa học</h4>
          <ul className="footer-links">
            <li><a href="#vocabulary">Từ vựng tiếng Anh</a></li>
            <li><a href="#ielts">Luyện thi IELTS</a></li>
            <li><a href="#listening">Luyện nghe</a></li>
            <li><a href="#toeic">Luyện thi TOEIC</a></li>
          </ul>
        </div>

        <div className="footer-section">
          <h4 className="footer-title">Tài liệu</h4>
          <ul className="footer-links">
            <li><a href="#vocab-basic">1000 Từ vựng cơ bản</a></li>
            <li><a href="#vocab-ielts">1200 Từ vựng IELTS</a></li>
            <li><a href="#vocab-toeic">1000 Từ vựng TOEIC</a></li>
            <li><a href="#phrasal-verbs">Cụm động từ</a></li>
          </ul>
        </div>

        <div className="footer-section">
          <h4 className="footer-title">Liên hệ</h4>
          <ul className="footer-links">
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
        </div>
      </div>

      <div className="footer-bottom">
        <p>&copy; 2025 Catalunya English. All rights reserved.</p>
        <div className="footer-legal">
          <a href="#privacy">Privacy Policy</a>
          <span>|</span>
          <a href="#terms">Terms of Service</a>
        </div>
      </div>
    </footer>
  );
}


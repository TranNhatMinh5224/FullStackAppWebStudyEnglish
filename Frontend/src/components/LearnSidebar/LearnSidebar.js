import React from 'react';
import { useNavigate } from 'react-router-dom';
import './LearnSidebar.css';
import { images } from '../../assets/images';

const LearnSidebar = ({ onShowCourseList }) => {
  const navigate = useNavigate();

  const handleShowTips = () => {
    navigate('/tips');
  };
  return (
    <aside className="learn-sidebar">
      {/* Quick Stats */}
      <div className="quick-stats-card">
        <div className="stats-header">
          <h3>Tiến độ học tập</h3>
        </div>
        <div className="stats-content">
          <div className="stat-item">
            <span className="stat-number">5</span>
            <span className="stat-label">Từ đã học</span>
          </div>
          <div className="stat-item">
            <span className="stat-number">2</span>
            <span className="stat-label">Khóa học</span>
          </div>
        </div>
      </div>

      {/* Course List Button */}
      <div className="course-list-card">
        <div className="course-icon-wrapper">
          <div className="course-icon">📚</div>
        </div>
        <h3>Khóa học của bạn</h3>
        <p>Khám phá các khóa học tiếng Anh phong phú</p>
        <button className="btn-show-courses" onClick={onShowCourseList}>
          Xem danh sách khóa học
        </button>
      </div>

      {/* Tips Ghi Nhớ Từ Vựng - Thiết kế đơn giản */}
      <div className="tips-card" onClick={handleShowTips}>
        <div className="tips-content">
          <h3>Tips Ghi Nhớ Từ Vựng nè ! ! !</h3>
        </div>
        <div className="tips-image">
          <img src={images.anhchotiptuvung} alt="Tips từ vựng" />
        </div>
      </div>
    </aside>
  );
};

export default LearnSidebar;
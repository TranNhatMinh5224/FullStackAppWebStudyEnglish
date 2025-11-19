import React from 'react';
import './StatsSidebar.css';

const StatsSidebar = () => {
  return (
    <aside className="stats-sidebar">
      {/* Thống kê học tập */}
      <div className="stat-card yellow">
        <p>Bạn đã học được</p>
        <h3>5 từ</h3>
      </div>

      <div className="stat-card green">
        <p>Học liên tục</p>
        <h3>0 ngày</h3>
      </div>

      {/* Progress section */}
      <div className="progress-section">
        <div className="progress-title">Tiến độ hôm nay</div>
        <div className="progress-bar">
          <div className="progress-fill"></div>
        </div>
        <div className="progress-text">5/20 từ mới</div>
      </div>

      {/* Achievement badges */}
      <div className="achievements">
        <div className="achievement-badge">
          <div className="achievement-icon">🏆</div>
          <div className="achievement-text">Người mới</div>
        </div>
        <div className="achievement-badge">
          <div className="achievement-icon">⭐</div>
          <div className="achievement-text">Siêng năng</div>
        </div>
        <div className="achievement-badge">
          <div className="achievement-icon">🔥</div>
          <div className="achievement-text">Streak</div>
        </div>
        <div className="achievement-badge">
          <div className="achievement-icon">💎</div>
          <div className="achievement-text">VIP</div>
        </div>
      </div>
    </aside>
  );
};

export default StatsSidebar;
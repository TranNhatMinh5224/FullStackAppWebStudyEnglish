import React, { useState } from 'react';
import './CourseListSidebar.css';

const CourseListSidebar = ({ isOpen, onClose, onCourseSelect }) => {
  // Mock data cho các khóa học
  const [courses] = useState([
    {
      id: 1,
      title: "Tiếng Anh Cơ Bản",
      description: "Học từ vựng và ngữ pháp cơ bản",
      level: "Beginner",
      lessons: 20,
      progress: 65,
      color: "#4CAF50"
    },
    {
      id: 2,
      title: "Tiếng Anh Giao Tiếp",
      description: "Luyện tập hội thoại hàng ngày",
      level: "Intermediate",
      lessons: 15,
      progress: 30,
      color: "#2196F3"
    },
    {
      id: 3,
      title: "Tiếng Anh Doanh Nghiệp",
      description: "Từ vựng và thuật ngữ business",
      level: "Advanced",
      lessons: 25,
      progress: 10,
      color: "#FF9800"
    },
    {
      id: 4,
      title: "TOEIC Preparation",
      description: "Luyện thi TOEIC hiệu quả",
      level: "Advanced",
      lessons: 30,
      progress: 80,
      color: "#9C27B0"
    },
    {
      id: 5,
      title: "Từ Vựng Chủ Đề",
      description: "Học từ vựng theo chủ đề cụ thể",
      level: "All Levels",
      lessons: 40,
      progress: 45,
      color: "#F44336"
    }
  ]);

  const handleCourseClick = (course) => {
    if (onCourseSelect) {
      onCourseSelect(course);
    }
    onClose();
  };

  const getLevelColor = (level) => {
    switch (level) {
      case 'Beginner': return '#4CAF50';
      case 'Intermediate': return '#FF9800';
      case 'Advanced': return '#F44336';
      default: return '#2196F3';
    }
  };

  if (!isOpen) return null;

  return (
    <div className="course-sidebar-overlay">
      <div className="course-sidebar">
        {/* Header */}
        <div className="course-sidebar-header">
          <h2>Danh Sách Khóa Học</h2>
          <button className="close-btn" onClick={onClose}>
            <span>×</span>
          </button>
        </div>

        {/* Course List */}
        <div className="course-list">
          {courses.map((course) => (
            <div 
              key={course.id} 
              className="course-card"
              onClick={() => handleCourseClick(course)}
            >
              <div className="course-header">
                <div 
                  className="course-icon"
                  style={{ backgroundColor: course.color }}
                >
                  📚
                </div>
                <div className="course-info">
                  <h3 className="course-title">{course.title}</h3>
                  <p className="course-description">{course.description}</p>
                </div>
              </div>

              <div className="course-details">
                <div className="course-meta">
                  <span 
                    className="course-level"
                    style={{ color: getLevelColor(course.level) }}
                  >
                    {course.level}
                  </span>
                  <span className="course-lessons">{course.lessons} bài học</span>
                </div>

                <div className="course-progress">
                  <div className="progress-bar">
                    <div 
                      className="progress-fill"
                      style={{ 
                        width: `${course.progress}%`,
                        backgroundColor: course.color
                      }}
                    ></div>
                  </div>
                  <span className="progress-text">{course.progress}%</span>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Footer */}
        <div className="course-sidebar-footer">
          <button className="browse-more-btn">
            Khám phá thêm khóa học
          </button>
        </div>
      </div>
    </div>
  );
};

export default CourseListSidebar;
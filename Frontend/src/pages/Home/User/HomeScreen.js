import React, { useState, useEffect } from "react";
import "./HomeScreen.css";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../../contexts/AuthContext";
import { 
  LoginModal, 
  HomeHeader, 
  LearnContent, 
  ReviewContent, 
  CourseListSidebar,
  StatsSidebar,
  LearnSidebar
} from "../../../components";

const HomeScreen = () => {
  const { isLoggedIn, isGuest } = useAuth();
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [activeTab, setActiveTab] = useState('learn'); // 'learn' hoặc 'review'
  const [showCourseList, setShowCourseList] = useState(false);
  const [selectedCourse, setSelectedCourse] = useState(null);
  const navigate = useNavigate();

  // Redirect to welcome if neither logged in nor guest
  useEffect(() => {
    if (!isLoggedIn && !isGuest) {
      navigate("/");
    }
  }, [isLoggedIn, isGuest, navigate]);

  const handleLearnClick = () => {
    if (isGuest) {
      setShowLoginModal(true);
    } else {
      // Navigate to learning page or start learning
      console.log("Start learning...");
    }
  };

  const handleTabChange = (tab) => {
    setActiveTab(tab);
  };

  const handleShowLoginModal = () => {
    setShowLoginModal(true);
  };

  const handleStartReview = () => {
    // Navigate to review page
    console.log("Start review...");
    // navigate("/review");
  };

  const handleLearnFromReview = () => {
    setActiveTab('learn');
  };

  const handleShowCourseList = () => {
    setShowCourseList(true);
  };

  const handleCloseCourseList = () => {
    setShowCourseList(false);
  };

  const handleCourseSelect = (course) => {
    setSelectedCourse(course);
    console.log("Selected course:", course);
    // Here you can navigate to course detail or start learning
  };

  // Don't render anything if not logged in or guest (will redirect anyway)
  if (!isLoggedIn && !isGuest) {
    return null;
  }

  return (
    <div className="home-container">
      <HomeHeader 
        activeTab={activeTab}
        onTabChange={handleTabChange}
        onShowLoginModal={handleShowLoginModal}
      />

      {/* Layout chính */}
      <div className="main-layout">
        {/* Render content dựa trên tab được chọn */}
        {activeTab === 'learn' ? (
          <LearnContent onLearnClick={handleLearnClick} />
        ) : (
          <ReviewContent 
            onShowLoginModal={handleShowLoginModal}
            onStartReview={handleStartReview}
          />
        )}

        {/* Sidebar phụ thuộc vào tab */}
        {activeTab === 'learn' ? (
          <LearnSidebar onShowCourseList={handleShowCourseList} />
        ) : (
          <StatsSidebar />
        )}
      </div>

      {/* Course List Sidebar */}
      <CourseListSidebar 
        isOpen={showCourseList}
        onClose={handleCloseCourseList}
        onCourseSelect={handleCourseSelect}
      />

      {/* Login Modal */}
      <LoginModal 
        isOpen={showLoginModal} 
        onClose={() => setShowLoginModal(false)}
      />
    </div>
  );
};

export default HomeScreen;

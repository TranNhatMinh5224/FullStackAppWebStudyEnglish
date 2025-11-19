import React from "react";
import { useAuth } from "../../contexts/AuthContext";
import { images } from "../../assets/images";
import "./ReviewContent.css";

const ReviewContent = ({ onShowLoginModal, onStartReview }) => {
  const { isLoggedIn, isGuest } = useAuth();

  const handleLearnNewWordsClick = () => {
    if (isGuest) {
      onShowLoginModal();
    } else {
      // Chuyển về tab học từ mới
      // Logic này sẽ được handle ở component cha
    }
  };

  const handleStartReviewClick = () => {
    if (isLoggedIn) {
      onStartReview();
    }
  };

  return (
    <div className="review-content">
      <div className="content-box">
        <img src={images.mascot1} alt="Mascot" className="mascot-big" />
        
        {isGuest ? (
          // Layout cho tài khoản khách
          <>
            <p className="review-message">
              Để kích hoạt tính năng ôn tập, hãy học 1 bài học
            </p>
            <button className="btn-learn-new" onClick={handleLearnNewWordsClick}>
              Học từ mới
            </button>
          </>
        ) : (
          // Layout cho người dùng đã đăng nhập
          <>
            <p className="review-message">
              Chuẩn bị ôn tập 5 từ 
            </p>
            <button className="btn-review-now" onClick={handleStartReviewClick}>
              Ôn tập ngay
            </button>
          </>
        )}
      </div>
    </div>
  );
};

export default ReviewContent;
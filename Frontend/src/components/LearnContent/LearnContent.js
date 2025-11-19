import React from "react";
import { images } from "../../assets/images";
import "./LearnContent.css";

const LearnContent = ({ onLearnClick }) => {
  return (
    <div className="learn-content">
      <div className="learn-content-box">
        <img src={images.mascot1} alt="Mascot" className="mascot-big" />
        <p className="slogan">Hãy chăm chỉ học từ mới mỗi ngày bạn nhé ❤️</p>
        <button className="btn-learn" onClick={onLearnClick}>
          Bắt đầu học ngay
        </button>
      </div>
    </div>
  );
};

export default LearnContent;
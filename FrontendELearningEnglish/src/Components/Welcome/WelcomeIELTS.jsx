import React from "react";
import "./WelcomeIELTS.css";
import { mochiWelcome } from "../../Assets";

const FACEBOOK_GROUP_URL = "https://web.facebook.com/groups/843825855021989";

export default function WelcomeIELTS() {
  const handleLearnMore = () => {
    window.open(FACEBOOK_GROUP_URL, "_blank");
  };

  return (
    <section className="welcome-ielts">
      <div className="ielts-content">
        <div className="ielts-left">
          <h2 className="ielts-title">
            Đạt 6.5 IELTS sau 1 khóa học<br />
            với Adaptive Learning
          </h2>
          <button 
            className="ielts-btn"
            onClick={handleLearnMore}
          >
            Tìm hiểu thêm
          </button>
        </div>
        <div className="ielts-right">
          <img 
            src={mochiWelcome} 
            alt="Mochi Welcome" 
            className="ielts-image"
          />
        </div>
      </div>
    </section>
  );
}


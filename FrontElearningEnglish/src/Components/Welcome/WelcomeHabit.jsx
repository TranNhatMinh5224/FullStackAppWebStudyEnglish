import React from "react";
import "./WelcomeHabit.css";
import { mochiLoading } from "../../Assets";

const FACEBOOK_GROUP_URL = "https://web.facebook.com/groups/843825855021989";

export default function WelcomeHabit() {
  const handleJoinGroup = () => {
    window.open(FACEBOOK_GROUP_URL, "_blank");
  };

  return (
    <section className="welcome-habit">
      <div className="habit-content">
        <div className="habit-left">
          <img 
            src={mochiLoading} 
            alt="Mochi Loading" 
            className="habit-image"
          />
        </div>
        <div className="habit-right">
          <h2 className="habit-title">
            Dễ dàng duy trì<br />
            thói quen học tiếng Anh
          </h2>
          <p className="habit-description">
            Tham gia thử thách 14 ngày để hình thành thói quen học và nhận các phần quà đặc biệt từ Catalunya English
          </p>
          <div className="habit-actions">
            <button 
              className="habit-btn primary"
              onClick={handleJoinGroup}
            >
              Gia nhập link nhóm
            </button>
            <a 
              href={FACEBOOK_GROUP_URL} 
              target="_blank" 
              rel="noopener noreferrer"
              className="habit-link"
            >
              Vào Group để nhận quà tặng MIỄN PHÍ
            </a>
          </div>
          <p className="habit-tagline">
            Học đúng trọng tâm, ôn tập thông minh, tăng band nhanh chóng!
          </p>
        </div>
      </div>
    </section>
  );
}


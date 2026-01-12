import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./Loading.css";
import Header from "../../Components/Header/LogoHeader";
import { mochiLoading } from "../../Assets";

export default function Loading() {
  const navigate = useNavigate();

  useEffect(() => {
    const timer = setTimeout(() => {
      navigate("/welcome");
    }, 2000);

    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div className="loading-container">
      {/* Header */}
      <Header />

      {/* Nội dung trung tâm */}
      <div className="loading-content">
        <img src={mochiLoading} alt="hello" className="loading-img" />

        <div className="loading-right">
          <h1 className="loading-text">
            Chào mừng bạn đến với <br /> Catalunya English
          </h1>

          {/* SPINNER XOAY XOAY */}
          <div className="loading-spinner"></div>
        </div>
      </div>
    </div>
  );
}

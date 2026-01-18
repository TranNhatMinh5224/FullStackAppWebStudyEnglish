import React, { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./Loading.css";
import Header from "../../Components/Header/LogoHeader";
import { useAssets } from "../../Context/AssetContext";

export default function Loading() {
  const navigate = useNavigate();
  const { getLogo } = useAssets();
  const logo = getLogo(); // Logo từ AssetContext

  useEffect(() => {
    const timer = setTimeout(() => {
      navigate("/home");
    }, 2000);

    return () => clearTimeout(timer);
  }, [navigate]);

  return (
    <div className="loading-container">
      {/* Header */}
      <Header />

      {/* Nội dung trung tâm */}
      <div className="loading-content">
        {logo && <img src={logo} alt="Catalunya English Logo" className="loading-img" />}

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

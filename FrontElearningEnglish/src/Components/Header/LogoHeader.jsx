import React from "react";
import { useNavigate } from "react-router-dom";
import "./LogoHeader.css";
import { useAssets } from "../../Context/AssetContext";

export default function Header() {
  const navigate = useNavigate();
  const { getLogo } = useAssets();
  const logo = getLogo();

  const handleLogoClick = () => {
    navigate("/home");
  };

  return (
    <div className="header d-flex align-items-center" onClick={handleLogoClick} style={{ cursor: "pointer" }}>
      {logo && <img src={logo} alt="logo" className="header-logo" />}
      <span className="header-title">Catalunya English</span>
    </div>
  );
}
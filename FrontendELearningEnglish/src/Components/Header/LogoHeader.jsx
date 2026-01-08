import React from "react";
import { useNavigate } from "react-router-dom";
import "./LogoHeader.css";
import { mochiWelcome } from "../../Assets";

export default function Header() {
  const navigate = useNavigate();

  const handleLogoClick = () => {
    navigate("/welcome");
  };

  return (
    <div className="header" onClick={handleLogoClick} style={{ cursor: "pointer" }}>
      <img src={mochiWelcome} alt="logo" className="header-logo" />
      <span className="header-title">Catalunya English</span>
    </div>
  );
}
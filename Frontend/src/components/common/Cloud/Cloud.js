import React from "react";
import "./Cloud.css";

const Cloud = ({ src, position = "top-left" }) => {
  if (src) {
    return <img src={src} alt="Cloud" className={`cloud ${position}`} />;
  }
  
  return (
    <div className={`cloud ${position}`}>
      <div className="cloud-shape"></div>
    </div>
  );
};

export default Cloud;
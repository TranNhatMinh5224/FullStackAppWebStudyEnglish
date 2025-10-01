import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import WelcomeScreen from "./pages/WelcomeScreen";
import IntroScreen from "./pages/IntroScreen";
import HomeScreen from "./pages/HomeScreen";
import LoginScreen from "./pages/LoginScreen";
import RegisterScreen from "./pages/RegisterScreen";

// Loading component
const LoadingSpinner = () => (
  <div style={{
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100vh',
    background: 'linear-gradient(135deg, #FFCB08 0%, #FFA000 100%)',
    fontFamily: 'var(--font-family)',
    fontSize: '18px',
    color: '#333'
  }}>
    Đang tải...
  </div>
);

// Main app content component
const AppContent = () => {
  const [showIntro, setShowIntro] = useState(true);
  const { loading } = useAuth();

  const handleIntroComplete = () => {
    setShowIntro(false);
  };

  if (loading) {
    return <LoadingSpinner />;
  }

  if (showIntro) {
    return <IntroScreen onIntroComplete={handleIntroComplete} />;
  }

  return (
    <Router>
      <Routes>
        <Route path="/" element={<WelcomeScreen />} />
        <Route path="/home" element={<HomeScreen />} />
        <Route path="/login" element={<LoginScreen />} />
        <Route path="/register" element={<RegisterScreen />} />
      </Routes>
    </Router>
  );
};

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;

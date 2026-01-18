import React from 'react';
import { useTheme } from '../../../Context/ThemeContext';
import { FaMoon, FaSun } from 'react-icons/fa';
import './ThemeToggle.css';

export default function ThemeToggle() {
    const { theme, toggleTheme, isDark } = useTheme();

    return (
        <button
            className="theme-toggle-btn"
            onClick={toggleTheme}
            aria-label={`Switch to ${isDark ? 'light' : 'dark'} theme`}
            title={`Switch to ${isDark ? 'light' : 'dark'} theme`}
        >
            {isDark ? (
                <FaSun className="theme-icon" />
            ) : (
                <FaMoon className="theme-icon" />
            )}
        </button>
    );
}

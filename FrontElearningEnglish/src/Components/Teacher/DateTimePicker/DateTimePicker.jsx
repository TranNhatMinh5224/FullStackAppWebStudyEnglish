import React, { useState, useRef, useEffect } from "react";
import { FaCalendarAlt } from "react-icons/fa";
import "./DateTimePicker.css";

export default function DateTimePicker({ 
  value, 
  onChange, 
  min, 
  max,
  placeholder = "dd/mm/yyyy",
  hasError = false,
  disabled = false,
  label = "",
  required = false,
  dateOnly = false
}) {
  const [isOpen, setIsOpen] = useState(false);
  const [inputValue, setInputValue] = useState("");
  const pickerRef = useRef(null);

  // Initialize from value prop
  useEffect(() => {
    if (value) {
      const dateObj = new Date(value);
      if (!isNaN(dateObj.getTime())) {
        // Format date: dd/mm/yyyy
        const day = String(dateObj.getDate()).padStart(2, "0");
        const month = String(dateObj.getMonth() + 1).padStart(2, "0");
        const year = dateObj.getFullYear();
        const formattedDate = `${day}/${month}/${year}`;
        setInputValue(formattedDate);
      } else {
        setInputValue("");
      }
    } else {
      setInputValue("");
    }
  }, [value]);

  // Close picker when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (pickerRef.current && !pickerRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen]);

  // Parse date string (dd/mm/yyyy) to Date object
  const parseDate = (dateStr) => {
    if (!dateStr) return null;
    const parts = dateStr.split("/");
    if (parts.length !== 3) return null;
    const [day, month, year] = parts.map(Number);
    if (isNaN(day) || isNaN(month) || isNaN(year)) return null;
    
    // Validate date
    if (day < 1 || day > 31 || month < 1 || month > 12 || year < 1900 || year > 2100) {
      return null;
    }
    
    const dateObj = new Date(year, month - 1, day);
    if (isNaN(dateObj.getTime())) return null;
    
    // Set time to start of day for dateOnly mode
    if (dateOnly) {
      dateObj.setHours(0, 0, 0, 0);
    }
    
    // Validate min/max
    if (min && dateObj < min) return null;
    if (max && dateObj > max) return null;
    
    return dateObj;
  };

  // Handle input change - user types directly
  const handleInputChange = (e) => {
    let input = e.target.value;
    
    // Remove all non-digit and non-slash characters
    input = input.replace(/[^\d/]/g, '');
    
    // Limit to format: dd/mm/yyyy (max 10 chars: dd/mm/yyyy)
    if (input.length > 10) {
      input = input.slice(0, 10);
    }
    
    // Extract digits only
    const digits = input.replace(/\//g, '');
    
    // Build formatted string - don't auto-format numbers, just add slashes
    let formatted = '';
    
    // Day part: allow 1-2 digits, don't auto-format yet
    if (digits.length >= 1) {
      formatted += digits[0];
    }
    if (digits.length >= 2) {
      formatted += digits[1];
    }
    
    // Add first slash: only if user typed it OR if we have 2 digits and more input
    const hasFirstSlash = input.includes('/');
    if (hasFirstSlash) {
      // User typed slash - preserve it
      if (!formatted.includes('/')) {
        formatted += '/';
      }
    } else if (digits.length > 2) {
      // Auto-add slash after 2 digits
      formatted += '/';
    }
    
    // Month part: allow 1-2 digits after first slash
    const afterFirstSlash = digits.length > 2 ? digits.slice(2) : '';
    if (afterFirstSlash.length >= 1) {
      formatted += afterFirstSlash[0];
    }
    if (afterFirstSlash.length >= 2) {
      formatted += afterFirstSlash[1];
    }
    
    // Add second slash: only if user typed it OR if we have month digits and more input
    const slashCount = (input.match(/\//g) || []).length;
    if (slashCount >= 2) {
      // User typed second slash - preserve it
      if (!formatted.endsWith('/')) {
        formatted += '/';
      }
    } else if (afterFirstSlash.length >= 2 && digits.length > 4) {
      // Auto-add slash after 2 digit month
      formatted += '/';
    } else if (afterFirstSlash.length === 1 && digits.length > 3) {
      // Auto-add slash after 1 digit month (user is typing year)
      formatted += '/';
    }
    
    // Year part: remaining digits (max 4)
    if (digits.length > 4) {
      const yearStart = afterFirstSlash.length >= 2 ? 4 : (afterFirstSlash.length === 1 ? 3 : 2);
      const yearDigits = digits.slice(yearStart, yearStart + 4);
      formatted += yearDigits;
    }
    
    setInputValue(formatted);
    
    // Try to parse and set date if valid (complete date entered)
    if (formatted.length === 10) {
      const dateObj = parseDate(formatted);
      if (dateObj) {
        onChange(dateObj);
      }
    } else if (formatted.length < 10) {
      // Clear date if incomplete
      onChange(null);
    }
  };

  // Handle input blur - validate and auto-format on blur
  const handleInputBlur = () => {
    // Auto-format: add leading zeros for day/month if needed
    let formatted = inputValue.trim();
    
    // If empty, do nothing
    if (!formatted) {
      return;
    }
    
    const parts = formatted.split('/');
    
    // Format day: add leading zero if single digit
    if (parts.length > 0 && parts[0] && parts[0].length === 1 && /^\d$/.test(parts[0])) {
      parts[0] = '0' + parts[0];
    }
    
    // Format month: add leading zero if single digit
    if (parts.length > 1 && parts[1] && parts[1].length === 1 && /^\d$/.test(parts[1])) {
      parts[1] = '0' + parts[1];
    }
    
    // Reconstruct formatted string
    formatted = parts.join('/');
    
    // Ensure we have proper format: dd/mm/yyyy
    if (parts.length === 3 && parts[0] && parts[1] && parts[2]) {
      // All parts exist, ensure proper format
      const day = parts[0].padStart(2, '0');
      const month = parts[1].padStart(2, '0');
      const year = parts[2];
      formatted = `${day}/${month}/${year}`;
    }
    
    // Update input value if changed
    if (formatted !== inputValue) {
      setInputValue(formatted);
    }
    
    // Try to parse and set date if valid
    if (formatted.length === 10) {
      const dateObj = parseDate(formatted);
      if (dateObj) {
        onChange(dateObj);
      }
    }
  };

  // Handle calendar icon click - open native date picker or calendar
  const handleCalendarClick = (e) => {
    e.stopPropagation();
    if (!disabled) {
      setIsOpen(!isOpen);
    }
  };

  // Handle date selection from calendar (if using native input)
  const handleDateSelect = (e) => {
    const selectedDate = e.target.value;
    if (selectedDate) {
      const dateObj = new Date(selectedDate);
      if (!isNaN(dateObj.getTime())) {
        // Format to dd/mm/yyyy
        const day = String(dateObj.getDate()).padStart(2, "0");
        const month = String(dateObj.getMonth() + 1).padStart(2, "0");
        const year = dateObj.getFullYear();
        const formattedDate = `${day}/${month}/${year}`;
        setInputValue(formattedDate);
        onChange(dateObj);
        setIsOpen(false);
      }
    }
  };

  // Get today's date in YYYY-MM-DD format for native date input
  const getTodayDate = () => {
    const today = min || new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, "0");
    const day = String(today.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  // Get max date in YYYY-MM-DD format
  const getMaxDate = () => {
    if (!max) return null;
    const year = max.getFullYear();
    const month = String(max.getMonth() + 1).padStart(2, "0");
    const day = String(max.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  // Get min date in YYYY-MM-DD format
  const getMinDate = () => {
    if (!min) return null;
    const year = min.getFullYear();
    const month = String(min.getMonth() + 1).padStart(2, "0");
    const day = String(min.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
  };

  // Convert input value to YYYY-MM-DD for native date input
  const getInputDateValue = () => {
    if (inputValue.length === 10) {
      const dateObj = parseDate(inputValue);
      if (dateObj) {
        const year = dateObj.getFullYear();
        const month = String(dateObj.getMonth() + 1).padStart(2, "0");
        const day = String(dateObj.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
      }
    }
    return "";
  };

  const isDateValid = inputValue.length === 10 && parseDate(inputValue) !== null;

  return (
    <div className="datetime-picker-wrapper" ref={pickerRef}>
      {label && (
        <label className={`datetime-picker-label ${required ? "required" : ""}`}>
          {label}
        </label>
      )}
      <div className={`datetime-picker-input-container ${hasError ? "has-error" : ""} ${disabled ? "disabled" : ""} ${isDateValid ? "valid" : ""}`}>
        <input
          type="text"
          className="datetime-picker-input"
          value={inputValue}
          onChange={handleInputChange}
          onBlur={handleInputBlur}
          onFocus={() => setIsOpen(false)} // Close calendar if open when focusing input
          placeholder={placeholder}
          maxLength={10}
          disabled={disabled}
        />
        <button
          type="button"
          className="datetime-picker-icon-btn"
          onClick={handleCalendarClick}
          disabled={disabled}
          title="Chọn ngày từ lịch"
        >
          <FaCalendarAlt className="datetime-picker-icon" />
        </button>
      </div>
      
      {/* Native date picker overlay (hidden, triggered by icon) */}
      {isOpen && !disabled && (
        <div className="datetime-picker-calendar-overlay">
          <div className="datetime-picker-calendar-popup">
            <div className="datetime-picker-calendar-header">
              <span>Chọn ngày</span>
              <button
                type="button"
                className="datetime-picker-close-btn"
                onClick={() => setIsOpen(false)}
              >
                ×
              </button>
            </div>
            <input
              type="date"
              className="datetime-picker-native-input"
              value={getInputDateValue()}
              onChange={handleDateSelect}
              min={getMinDate() || undefined}
              max={getMaxDate() || undefined}
              autoFocus
            />
            <div className="datetime-picker-hint">
              Hoặc nhập trực tiếp: {placeholder}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

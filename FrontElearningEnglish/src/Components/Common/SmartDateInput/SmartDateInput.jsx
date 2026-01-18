import React, { useState, useEffect, useRef } from "react";
import PropTypes from "prop-types";
import "./SmartDateInput.css";

function pad(n) {
  return String(n).padStart(2, "0");
}

function parseDateFromString(text) {
  const digits = text.replace(/[^0-9]/g, "");
  // Prefer formats where year is 4 digits at end (e.g. 112026 -> 1/1/2026 or 0112026)
  if (digits.length < 6 || digits.length > 8) return null;
  const year = parseInt(digits.slice(-4), 10);
  const prefix = digits.slice(0, -4);

  // try possible splits of prefix into day/month (dayLen 1..2, monthLen 1..2)
  for (let dayLen = 1; dayLen <= 2; dayLen++) {
    const monthLen = prefix.length - dayLen;
    if (monthLen < 1 || monthLen > 2) continue;
    const dayStr = prefix.slice(0, dayLen);
    const monthStr = prefix.slice(dayLen);
    const dd = parseInt(dayStr, 10);
    const mm = parseInt(monthStr, 10) - 1;
    if (Number.isNaN(dd) || Number.isNaN(mm) || Number.isNaN(year)) continue;
    if (dd < 1 || dd > 31) continue;
    if (mm < 0 || mm > 11) continue;
    const d = new Date(year, mm, dd);
    if (d && d.getFullYear() === year && d.getMonth() === mm && d.getDate() === dd) return d;
  }

  return null;
}

export default function SmartDateInput({
  label,
  value,
  onChange,
  minDate,
  compareAfter,
  required,
  placeholder,
}) {
  const [text, setText] = useState("");
  const [error, setError] = useState(null);
  const inputRef = useRef(null);

  useEffect(() => {
    if (value instanceof Date && !isNaN(value.getTime())) {
      const day = pad(value.getDate());
      const month = pad(value.getMonth() + 1);
      const year = value.getFullYear();
      setText(`${day}/${month}/${year}`);
      setError(null);
    } else {
      if (text !== "") setText("");
      setError(null);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [value]);

  const handleChange = (e) => {
    let v = e.target.value;
    // Allow only digits and '/'
    v = v.replace(/[^0-9/]/g, "");
    // Remove duplicated slashes
    v = v.replace(/\/+/g, "/");
    // Remove leading slashes
    v = v.replace(/^\/+/, "");
    const digits = v.replace(/[^0-9]/g, "");

    
    if (digits.length <= 2) {
      v = digits;
    } else if (digits.length === 3) {
      // keep raw to avoid invalid-looking month like '11/0'
      v = digits;
    } else if (digits.length === 4) {
      v = `${digits.slice(0, 2)}/${digits.slice(2)}`;
    } else {
      const yr = digits.slice(-4);
      const prefix = digits.slice(0, -4);
      // try splitting prefix into day/month for display; prefer dayLen=1 then 2
      let day = prefix;
      let month = "";
      if (prefix.length === 2) {
        // ambiguous: try 1+1 first
        day = prefix.slice(0, 1);
        month = prefix.slice(1);
        const dd = parseInt(day, 10);
        const mm = parseInt(month, 10);
        if (!(dd >= 1 && dd <= 31 && mm >= 1 && mm <= 12)) {
          day = prefix.slice(0, 2);
          month = "";
        }
      } else if (prefix.length === 3) {
        day = prefix.slice(0, 1);
        month = prefix.slice(1);
      } else if (prefix.length === 1) {
        day = prefix;
        month = "";
      }
      if (month) v = `${day}/${month}/${yr}`;
      else v = `${day}/${yr}`;
    }

    if (v.length > 10) v = v.slice(0, 10);

    setText(v);

    // If full date entered, try to parse
    const parsed = parseDateFromString(v);
    if (parsed) {
      // Validate minDate (copy to avoid mutating prop)
      if (minDate) {
        const min = new Date(minDate);
        min.setHours(0, 0, 0, 0);
        if (parsed < min) {
          setError("Ngày không được trước ngày cho phép");
          return;
        }
      }

      // Validate compareAfter
      if (compareAfter && compareAfter instanceof Date && !isNaN(compareAfter.getTime())) {
        const ca = new Date(compareAfter);
        ca.setHours(0, 0, 0, 0);
        const p = new Date(parsed);
        p.setHours(0, 0, 0, 0);
        if (p <= ca) {
          setError("Thời gian phải sau thời gian so sánh");
          return;
        }
      }

      setError(null);
      onChange && onChange(parsed);
    } else {
      // Partial input - don't show error yet and don't clear parent's value
      setError(null);
      // do not call onChange(null) here to avoid parent resetting value while typing
    }
  };

  const handleBlur = () => {
    if (!text) {
      if (required) setError("Trường này là bắt buộc");
      onChange && onChange(null);
      return;
    }
    const parsed = parseDateFromString(text);
    if (!parsed) {
      setError("Ngày không hợp lệ (dd/mm/yyyy)");
      onChange && onChange(null);
      return;
    }
    // Final validation same as onChange
    if (minDate) {
      const min = new Date(minDate);
      min.setHours(0, 0, 0, 0);
      if (parsed < min) {
        setError("Ngày không được trước ngày cho phép");
        onChange && onChange(null);
        return;
      }
    }
    if (compareAfter && compareAfter instanceof Date && !isNaN(compareAfter.getTime())) {
      const ca = new Date(compareAfter);
      ca.setHours(0, 0, 0, 0);
      const p = new Date(parsed);
      p.setHours(0, 0, 0, 0);
      if (p <= ca) {
        setError("Thời gian phải sau thời gian so sánh");
        onChange && onChange(null);
        return;
      }
    }
    setError(null);
    onChange && onChange(parsed);
  };

  return (
    <div className="form-group smart-date-group">
      <label className={`form-label ${required ? 'required' : ''}`}>{label} {required && <span className="text-danger">*</span>}</label>
      <input
        ref={inputRef}
        type="text"
        className={`form-control smart-date-input ${error ? 'is-invalid' : ''}`}
        value={text}
        onChange={handleChange}
        onBlur={handleBlur}
        placeholder={placeholder || 'dd/mm/yyyy'}
        maxLength={10}
      />
      {error && <div className="invalid-feedback">{error}</div>}
    </div>
  );
}

SmartDateInput.propTypes = {
  label: PropTypes.string,
  value: PropTypes.instanceOf(Date),
  onChange: PropTypes.func,
  minDate: PropTypes.instanceOf(Date),
  compareAfter: PropTypes.instanceOf(Date),
  required: PropTypes.bool,
  placeholder: PropTypes.string,
};

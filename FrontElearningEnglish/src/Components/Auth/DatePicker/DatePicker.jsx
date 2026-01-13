import React, { useState, useEffect } from "react";
import SelectField from "../SelectField/SelectField";
import { Row, Col } from "react-bootstrap";
import "./DatePicker.css";

export default function DatePicker({ value, onChange, disabled = false, hasError = false }) {
    const [day, setDay] = useState("");
    const [month, setMonth] = useState("");
    const [year, setYear] = useState("");

    // Parse date value (ISO string or Date object)
    useEffect(() => {
        if (value) {
            const date = new Date(value);
            if (!isNaN(date.getTime())) {
                setDay(date.getDate().toString());
                setMonth((date.getMonth() + 1).toString());
                setYear(date.getFullYear().toString());
            }
        } else {
            setDay("");
            setMonth("");
            setYear("");
        }
    }, [value]);

    // Get days in month
    const getDaysInMonth = (m, y) => {
        if (!m || !y) return 31;
        return new Date(parseInt(y), parseInt(m), 0).getDate();
    };

    // Generate options
    const generateDays = () => {
        const maxDays = (month && year) ? getDaysInMonth(month, year) : 31;
        return Array.from({ length: maxDays }, (_, i) => ({
            value: (i + 1).toString(),
            label: (i + 1).toString(),
        }));
    };

    const generateMonths = () => {
        return Array.from({ length: 12 }, (_, i) => ({
            value: (i + 1).toString(),
            label: (i + 1).toString(),
        }));
    };

    const generateYears = () => {
        const currentYear = new Date().getFullYear();
        return Array.from({ length: 100 }, (_, i) => {
            const y = currentYear - i;
            return {
                value: y.toString(),
                label: y.toString(),
            };
        });
    };

    // Handle changes
    const handleDayChange = (newDay) => {
        setDay(newDay);
        if (newDay && month && year) {
            const date = new Date(parseInt(year), parseInt(month) - 1, parseInt(newDay));
            onChange(date);
        }
    };

    const handleMonthChange = (newMonth) => {
        setMonth(newMonth);

        // Only update date if all three values are present
        if (day && newMonth && year) {
            const maxDays = getDaysInMonth(newMonth, year);
            // Adjust day if it exceeds max days in the new month
            if (parseInt(day) > maxDays) {
                setDay(maxDays.toString());
                const date = new Date(parseInt(year), parseInt(newMonth) - 1, maxDays);
                onChange(date);
            } else {
                const date = new Date(parseInt(year), parseInt(newMonth) - 1, parseInt(day));
                onChange(date);
            }
        }
        // If not all values present, don't reset - just wait for complete date
    };

    const handleYearChange = (newYear) => {
        setYear(newYear);

        // Only update date if all three values are present
        if (day && month && newYear) {
            const maxDays = getDaysInMonth(month, newYear);
            // Adjust day if it exceeds max days in the selected month/year
            if (parseInt(day) > maxDays) {
                setDay(maxDays.toString());
                const date = new Date(parseInt(newYear), parseInt(month) - 1, maxDays);
                onChange(date);
            } else {
                const date = new Date(parseInt(newYear), parseInt(month) - 1, parseInt(day));
                onChange(date);
            }
        }
        // If not all values present, don't reset - just wait for complete date
    };

    return (
        <Row className="date-picker-container gx-2">
            <Col xs={4}>
                <SelectField
                    options={generateDays()}
                    value={day}
                    onChange={(e) => handleDayChange(e.target.value)}
                    disabled={disabled}
                    placeholder="Ngày"
                    error={hasError}
                />
            </Col>
            <Col xs={4}>
                <SelectField
                    options={generateMonths()}
                    value={month}
                    onChange={(e) => handleMonthChange(e.target.value)}
                    disabled={disabled}
                    placeholder="Tháng"
                    error={hasError}
                />
            </Col>
            <Col xs={4}>
                <SelectField
                    options={generateYears()}
                    value={year}
                    onChange={(e) => handleYearChange(e.target.value)}
                    disabled={disabled}
                    placeholder="Năm"
                    error={hasError}
                />
            </Col>
        </Row>
    );
}
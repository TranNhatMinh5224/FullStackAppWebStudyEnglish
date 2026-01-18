import React, { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { FaSearch, FaUsers } from "react-icons/fa";
import { courseService } from "../../../Services/courseService";
import { useAssets } from "../../../Context/AssetContext";
import "./SearchBox.css";

export default function SearchBox() {
    const [searchQuery, setSearchQuery] = useState("");
    const [searchResults, setSearchResults] = useState([]);
    const [isSearching, setIsSearching] = useState(false);
    const [showDropdown, setShowDropdown] = useState(false);
    const [selectedIndex, setSelectedIndex] = useState(-1);
    const searchTimeoutRef = useRef(null);
    const dropdownRef = useRef(null);
    const navigate = useNavigate();
    const { getDefaultCourseImage } = useAssets();
    const defaultCourseImage = getDefaultCourseImage();

    useEffect(() => {
        // Clear previous timeout
        if (searchTimeoutRef.current) {
            clearTimeout(searchTimeoutRef.current);
        }

        // If search query is empty, clear results
        if (!searchQuery.trim()) {
            setSearchResults([]);
            setShowDropdown(false);
            return;
        }

        // Debounce search - wait 300ms after user stops typing
        setIsSearching(true);
        searchTimeoutRef.current = setTimeout(async () => {
            try {
                const response = await courseService.searchCourses(searchQuery.trim());
                const courses = response.data?.data || [];
                
                const mappedCourses = courses.map((course) => ({
                    id: course.courseId,
                    courseId: course.courseId,
                    title: course.title || course.Title || "",
                    description: course.description || course.Description || "",
                    imageUrl: course.imageUrl && course.imageUrl.trim() !== "" 
                        ? course.imageUrl 
                        : defaultCourseImage,
                    price: course.price || course.Price || 0,
                    enrollmentCount: course.enrollmentCount || course.EnrollmentCount || 0,
                }));
                
                setSearchResults(mappedCourses);
                setShowDropdown(mappedCourses.length > 0);
            } catch (error) {
                console.error("Error searching courses:", error);
                setSearchResults([]);
                setShowDropdown(false);
            } finally {
                setIsSearching(false);
            }
        }, 300);

        return () => {
            if (searchTimeoutRef.current) {
                clearTimeout(searchTimeoutRef.current);
            }
        };
    }, [searchQuery]);

    // Close dropdown when clicking outside
    useEffect(() => {
        const handleClickOutside = (event) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setShowDropdown(false);
            }
        };

        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    // Highlight keyword in text
    const highlightKeyword = (text, keyword) => {
        if (!text || !keyword) return text;
        const regex = new RegExp(`(${keyword})`, 'gi');
        const parts = text.split(regex);
        return parts.map((part, index) => 
            regex.test(part) ? (
                <mark key={index} className="search-highlight">{part}</mark>
            ) : part
        );
    };

    // Truncate description
    const truncateDescription = (text, maxLength = 80) => {
        if (!text) return "";
        if (text.length <= maxLength) return text;
        return text.substring(0, maxLength) + "...";
    };

    // Format price
    const formatPrice = (price) => {
        if (!price || price === 0) return "Miễn phí";
        return `${price.toLocaleString("vi-VN")}đ`;
    };

    // Handle course click
    const handleCourseClick = (courseId) => {
        setShowDropdown(false);
        setSearchQuery("");
        navigate(`/course/${courseId}`);
    };

    // Handle keyboard navigation
    useEffect(() => {
        const handleKeyDown = (e) => {
            if (!showDropdown || searchResults.length === 0) return;

            switch (e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    setSelectedIndex(prev => 
                        prev < searchResults.length - 1 ? prev + 1 : prev
                    );
                    break;
                case 'ArrowUp':
                    e.preventDefault();
                    setSelectedIndex(prev => prev > 0 ? prev - 1 : -1);
                    break;
                case 'Enter':
                    e.preventDefault();
                    if (selectedIndex >= 0 && selectedIndex < searchResults.length) {
                        const course = searchResults[selectedIndex];
                        setShowDropdown(false);
                        setSearchQuery("");
                        navigate(`/course/${course.courseId || course.id}`);
                    }
                    break;
                case 'Escape':
                    setShowDropdown(false);
                    setSelectedIndex(-1);
                    break;
                default:
                    break;
            }
        };

        if (showDropdown) {
            document.addEventListener('keydown', handleKeyDown);
            return () => document.removeEventListener('keydown', handleKeyDown);
        }
    }, [showDropdown, searchResults, selectedIndex, navigate]);

    // Reset selected index when results change
    useEffect(() => {
        setSelectedIndex(-1);
    }, [searchResults]);

    return (
        <div className="search-box-container" ref={dropdownRef}>
            <div className="search-box d-flex align-items-center">
                <FaSearch className="search-icon" />
                <input
                    type="text"
                    placeholder="Tìm kiếm khóa học..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    onFocus={() => {
                        if (searchResults.length > 0) {
                            setShowDropdown(true);
                        }
                    }}
                />
            </div>
            {showDropdown && (
                <div className="search-dropdown">
                    {isSearching ? (
                        <div className="search-loading">
                            <div className="loading-spinner"></div>
                            <span>Đang tìm kiếm...</span>
                        </div>
                    ) : searchResults.length > 0 ? (
                        <>
                            <div className="search-results">
                                {searchResults.map((course, index) => (
                                    <div
                                        key={course.id}
                                        className={`search-result-item ${selectedIndex === index ? 'selected' : ''}`}
                                        onClick={() => handleCourseClick(course.courseId || course.id)}
                                        onMouseEnter={() => setSelectedIndex(index)}
                                    >
                                        <div className="result-image-wrapper">
                                            <img
                                                src={course.imageUrl}
                                                alt={course.title}
                                                className="search-result-image"
                                            />
                                        </div>
                                        <div className="result-content">
                                            <h5 className="result-title">
                                                {highlightKeyword(course.title, searchQuery.trim())}
                                            </h5>
                                            {course.description && (
                                                <p className="result-description">
                                                    {highlightKeyword(truncateDescription(course.description), searchQuery.trim())}
                                                </p>
                                            )}
                                            <div className="result-meta">
                                                <span className="result-price">{formatPrice(course.price)}</span>
                                                {course.enrollmentCount > 0 && (
                                                    <span className="result-enrollment">
                                                        <FaUsers /> {course.enrollmentCount} học viên
                                                    </span>
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </>
                    ) : (
                        <div className="search-no-results">
                            <div className="no-results-icon">🔍</div>
                            <p>Không tìm thấy khóa học nào</p>
                            <span className="no-results-hint">Thử tìm kiếm với từ khóa khác</span>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}

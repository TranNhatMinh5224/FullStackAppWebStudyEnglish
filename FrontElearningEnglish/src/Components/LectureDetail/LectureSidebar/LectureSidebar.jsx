import React, { useState, useEffect } from "react";
import { FaChevronDown, FaChevronRight } from "react-icons/fa";
import "./LectureSidebar.css";

export default function LectureSidebar({ lectureTree, currentLectureId, onLectureClick }) {
    const [expandedItems, setExpandedItems] = useState(new Set());

    // Auto-expand items that contain the current lecture
    useEffect(() => {
        if (currentLectureId && lectureTree.length > 0) {
            const findParentIds = (items, targetId, parentIds = []) => {
                for (const item of items) {
                    const itemId = item.lectureId || item.LectureId;
                    const children = item.children || item.Children || [];
                    
                    if (itemId === targetId) {
                        return parentIds;
                    }
                    
                    if (children.length > 0) {
                        const result = findParentIds(children, targetId, [...parentIds, itemId]);
                        if (result !== null) {
                            return result;
                        }
                    }
                }
                return null;
            };

            const parentIds = findParentIds(lectureTree, currentLectureId);
            if (parentIds) {
                setExpandedItems(new Set(parentIds));
            }
        }
    }, [currentLectureId, lectureTree]);


    const renderLectureItem = (lecture, level = 0) => {
        const lectureId = lecture.lectureId || lecture.LectureId;
        const title = lecture.title || lecture.Title || "";
        const numberingLabel = lecture.numberingLabel || lecture.NumberingLabel || "";
        const children = lecture.children || lecture.Children || [];
        const hasChildren = children.length > 0;
        const isExpanded = expandedItems.has(lectureId);
        const isActive = lectureId === currentLectureId;
        const type = lecture.type || lecture.Type || 1;
        const typeName = lecture.typeName || lecture.TypeName || "Content";

        const displayLabel = numberingLabel || `${lecture.orderIndex || lecture.OrderIndex || ""}`;
        const displayTitle = title;

        // Handle click: expand/collapse if has children, otherwise load lecture
        const handleClick = () => {
            if (hasChildren) {
                // Toggle expand when clicking parent
                const newExpanded = new Set(expandedItems);
                if (newExpanded.has(lectureId)) {
                    newExpanded.delete(lectureId);
                } else {
                    newExpanded.add(lectureId);
                }
                setExpandedItems(newExpanded);
            } else {
                // Load lecture content for leaf nodes
                onLectureClick(lectureId);
            }
        };

        return (
            <div key={lectureId} className="lecture-tree-item">
                <div
                    className={`lecture-tree-node ${isActive ? "active" : ""} ${hasChildren ? "has-children" : "leaf-node"} level-${level}`}
                    onClick={handleClick}
                >
                    <span className="expand-icon-placeholder" />
                    <span className="lecture-numbering">{displayLabel}</span>
                    <span className="lecture-title-text" title={displayTitle}>{displayTitle}</span>
                    {hasChildren && (
                        <span className="expand-icon">
                            {isExpanded ? <FaChevronDown /> : <FaChevronRight />}
                        </span>
                    )}
                </div>
                {hasChildren && isExpanded && (
                    <div className="lecture-tree-children">
                        {children.map((child) => renderLectureItem(child, level + 1))}
                    </div>
                )}
            </div>
        );
    };


    return (
        <div className="lecture-sidebar">
            <div className="sidebar-header">
                <h3>Mục lục bài giảng</h3>
            </div>
            <div className="sidebar-content">
                {lectureTree.length > 0 ? (
                    lectureTree.map((lecture) => renderLectureItem(lecture, 0))
                ) : (
                    <div className="no-lectures-message">Chưa có bài giảng nào</div>
                )}
            </div>
        </div>
    );
}

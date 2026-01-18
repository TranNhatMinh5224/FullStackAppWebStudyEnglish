import React, { useState, useEffect, useRef } from "react";
import { FaChevronDown, FaChevronRight, FaCheckCircle, FaCircle } from "react-icons/fa";
import "./LectureTree.css";

const LectureTree = ({ lectureTree, currentLectureId, onLectureClick }) => {
    const [expandedItems, setExpandedItems] = useState(new Set());
    const treeContentRef = useRef(null);
    const activeItemRef = useRef(null);
    const hasInitializedRef = useRef(false); // Track if we've done initial auto-expand

    // Reset initialization when tree changes (e.g., switching modules)
    useEffect(() => {
        hasInitializedRef.current = false;
        setExpandedItems(new Set()); // Reset expanded items when tree changes
    }, [lectureTree]);

    // Auto-expand items that contain the current lecture ONLY on initial load
    // After that, user controls expand/collapse manually
    useEffect(() => {
        if (currentLectureId && lectureTree.length > 0 && !hasInitializedRef.current) {
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
            
            hasInitializedRef.current = true;
        }
    }, [currentLectureId, lectureTree]);

    // Scroll to active item when currentLectureId changes (but don't change expanded state)
    useEffect(() => {
        if (currentLectureId && lectureTree.length > 0) {
            setTimeout(() => {
                if (activeItemRef.current && treeContentRef.current) {
                    const activeElement = activeItemRef.current;
                    const treeContent = treeContentRef.current;
                    
                    const activeTop = activeElement.offsetTop;
                    const activeHeight = activeElement.offsetHeight;
                    const treeHeight = treeContent.clientHeight;
                    const treeScrollTop = treeContent.scrollTop;
                    
                    const isVisible = activeTop >= treeScrollTop && 
                                    (activeTop + activeHeight) <= (treeScrollTop + treeHeight);
                    
                    if (!isVisible) {
                        const scrollTo = activeTop - (treeHeight / 2) + (activeHeight / 2);
                        treeContent.scrollTo({
                            top: Math.max(0, scrollTo),
                            behavior: 'smooth'
                        });
                    }
                }
            }, 100);
        }
    }, [currentLectureId, lectureTree]);

    const renderTreeItem = (lecture, level = 0) => {
        const lectureId = lecture.lectureId || lecture.LectureId;
        const title = lecture.title || lecture.Title || "";
        const numberingLabel = lecture.numberingLabel || lecture.NumberingLabel || "";
        const children = lecture.children || lecture.Children || [];
        const hasChildren = children.length > 0;
        const isExpanded = expandedItems.has(lectureId);
        const isActive = lectureId === currentLectureId;

        const displayLabel = numberingLabel || `${lecture.orderIndex || lecture.OrderIndex || ""}`;
        const displayTitle = title;

        const handleClick = (e) => {
            if (hasChildren) {
                // Toggle expand/collapse - only when user clicks
                e.stopPropagation(); // Prevent any parent handlers
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
            <div key={lectureId} className="tree-item">
                <div
                    ref={isActive ? activeItemRef : null}
                    className={`tree-node ${isActive ? "active" : ""} ${hasChildren ? "has-children" : "leaf-node"} level-${level}`}
                    onClick={handleClick}
                >
                    {!hasChildren && (
                        <span className="status-icon">
                            {isActive ? <FaCheckCircle className="status-completed" /> : <FaCircle className="status-pending" />}
                        </span>
                    )}
                    {hasChildren && <span className="expand-icon-placeholder" />}
                    <span className="tree-numbering">{displayLabel}</span>
                    <span className="tree-title" title={displayTitle}>{displayTitle}</span>
                    {hasChildren && (
                        <span className="expand-icon">
                            {isExpanded ? <FaChevronDown /> : <FaChevronRight />}
                        </span>
                    )}
                </div>
                {hasChildren && isExpanded && (
                    <div className="tree-children">
                        {children.map((child) => renderTreeItem(child, level + 1))}
                    </div>
                )}
            </div>
        );
    };

    return (
        <div className="lecture-tree">
            <div ref={treeContentRef} className="tree-content">
                {lectureTree.length > 0 ? (
                    lectureTree.map((lecture) => renderTreeItem(lecture, 0))
                ) : (
                    <div className="no-lectures-message">Chưa có bài giảng nào</div>
                )}
            </div>
        </div>
    );
};

LectureTree.displayName = "LectureTree";

export default LectureTree;

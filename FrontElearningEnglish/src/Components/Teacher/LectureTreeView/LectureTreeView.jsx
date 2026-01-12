import React, { useState, useMemo } from "react";
import { Card, Button, Badge, Collapse, OverlayTrigger, Tooltip } from "react-bootstrap";
import { 
  FaChevronDown, 
  FaChevronRight, 
  FaPlus, 
  FaEdit, 
  FaTrash, 
  FaBook, 
  FaVideo, 
  FaFileAlt, 
  FaEye,
  FaSitemap,
  FaClock
} from "react-icons/fa";
import { useEnums } from "../../../Context/EnumContext";
import "./LectureTreeView.css";

/**
 * LectureTreeView Component - Hiển thị lectures dạng tree với khả năng tạo con, edit, delete
 * Cải thiện UI/UX để dễ nhận biết cấu trúc cha-con
 */
export default function LectureTreeView({ 
  lectures = [], 
  onAddChild, 
  onEdit, 
  onDelete,
  onView 
}) {
  const { getEnumLabel } = useEnums();

  // Tính tổng số lectures (bao gồm cả con)
  const totalCount = useMemo(() => {
    const countAll = (items) => {
      return items.reduce((sum, item) => {
        const childCount = item.children ? countAll(item.children) : 0;
        return sum + 1 + childCount;
      }, 0);
    };
    return countAll(lectures);
  }, [lectures]);

  // Helper functions for lecture type display
  const getLectureIcon = (type) => {
    const typeValue = typeof type === 'number' ? type : parseInt(type);
    const typeName = getEnumLabel("LectureType", typeValue)?.toLowerCase() || "";
    
    if (!typeName) {
      if (typeValue === 1) return <FaBook />;
      if (typeValue === 2) return <FaFileAlt />;
      if (typeValue === 3) return <FaVideo />;
    }
    
    if (typeName === "content") return <FaBook />;
    if (typeName === "document") return <FaFileAlt />;
    if (typeName === "video") return <FaVideo />;
    return <FaBook />;
  };

  const getLectureLabel = (type) => {
    if (type === null || type === undefined || type === '') return "Nội dung";
    const typeValue = typeof type === 'number' ? type : parseInt(type);
    if (isNaN(typeValue)) return "Nội dung";
    
    const typeName = getEnumLabel("LectureType", typeValue) || "";
    if (!typeName || typeName === 'Unknown') {
      const fallbackLabels = { 1: "Nội dung", 2: "Tài liệu", 3: "Video" };
      return fallbackLabels[typeValue] || "Nội dung";
    }
    
    const labels = { "Content": "Nội dung", "Document": "Tài liệu", "Video": "Video" };
    return labels[typeName] || "Nội dung";
  };

  const getLectureBadgeVariant = (type) => {
    if (type === null || type === undefined || type === '') return "primary";
    const typeValue = typeof type === 'number' ? type : parseInt(type);
    if (isNaN(typeValue)) return "primary";
    
    const typeName = getEnumLabel("LectureType", typeValue)?.toLowerCase() || "";
    if (!typeName || typeName === 'unknown') {
      if (typeValue === 1) return "primary";
      if (typeValue === 2) return "info";
      if (typeValue === 3) return "danger";
      return "primary";
    }
    
    if (typeName === "content") return "primary";
    if (typeName === "document") return "info";
    if (typeName === "video") return "danger";
    return "primary";
  };

  // Color theo level - giúp phân biệt độ sâu
  const getLevelColor = (level) => {
    const colors = [
      { border: '#0d6efd', bg: 'rgba(13, 110, 253, 0.05)', text: '#0d6efd' },
      { border: '#198754', bg: 'rgba(25, 135, 84, 0.05)', text: '#198754' },
      { border: '#fd7e14', bg: 'rgba(253, 126, 20, 0.05)', text: '#fd7e14' },
      { border: '#6f42c1', bg: 'rgba(111, 66, 193, 0.05)', text: '#6f42c1' },
      { border: '#20c997', bg: 'rgba(32, 201, 151, 0.05)', text: '#20c997' },
    ];
    return colors[Math.min(level, colors.length - 1)];
  };

  return (
    <div className="ltv-container">
      {/* Header thống kê */}
      <div className="ltv-header">
        <div className="ltv-header__stats">
          <FaSitemap className="ltv-header__icon" />
          <span className="ltv-header__text">
            <strong>{lectures.length}</strong> bài gốc • <strong>{totalCount}</strong> tổng cộng
          </span>
        </div>
        <div className="ltv-header__legend">
          <span className="ltv-legend-item ltv-legend-item--level0">Gốc</span>
          <span className="ltv-legend-item ltv-legend-item--level1">Cấp 1</span>
          <span className="ltv-legend-item ltv-legend-item--level2">Cấp 2</span>
          <span className="ltv-legend-item ltv-legend-item--level3">Cấp 3+</span>
        </div>
      </div>

      {/* Tree content */}
      {lectures.length === 0 ? (
        <div className="ltv-empty">
          <FaBook className="ltv-empty__icon" />
          <p className="ltv-empty__text">Chưa có bài giảng nào</p>
          <p className="ltv-empty__hint">Nhấn "Tạo Lecture mới" để bắt đầu</p>
        </div>
      ) : (
        <div className="ltv-tree">
          {lectures.map((lecture, index) => (
            <LectureTreeNode
              key={lecture.lectureId || lecture.LectureId}
              lecture={lecture}
              level={0}
              isLast={index === lectures.length - 1}
              parentTitle={null}
              onAddChild={onAddChild}
              onEdit={onEdit}
              onDelete={onDelete}
              onView={onView}
              getLectureIcon={getLectureIcon}
              getLectureLabel={getLectureLabel}
              getLectureBadgeVariant={getLectureBadgeVariant}
              getLevelColor={getLevelColor}
            />
          ))}
        </div>
      )}
    </div>
  );
}

/**
 * LectureTreeNode - Component cho mỗi node trong tree
 */
function LectureTreeNode({ 
  lecture, 
  level = 0,
  isLast = false,
  parentTitle = null,
  onAddChild, 
  onEdit, 
  onDelete,
  onView,
  getLectureIcon,
  getLectureLabel,
  getLectureBadgeVariant,
  getLevelColor
}) {
  const [expanded, setExpanded] = useState(level < 2);
  
  const hasChildren = lecture.children && lecture.children.length > 0;
  const title = lecture.title || lecture.Title;
  const type = lecture.type || lecture.Type;
  const duration = lecture.duration || lecture.Duration;
  const numberingLabel = lecture.numberingLabel || lecture.NumberingLabel;
  const levelColor = getLevelColor(level);

  const handleToggle = () => {
    if (hasChildren) {
      setExpanded(!expanded);
    }
  };

  const handleView = (e) => {
    e.stopPropagation();
    onView?.(lecture);
  };

  const handleAddChild = (e) => {
    e.stopPropagation();
    onAddChild?.(lecture);
  };

  const handleEdit = (e) => {
    e.stopPropagation();
    onEdit?.(lecture);
  };

  const handleDelete = (e) => {
    e.stopPropagation();
    onDelete?.(lecture);
  };

  // Format duration
  const formatDuration = (seconds) => {
    if (!seconds) return null;
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  return (
    <div className={`ltv-node ltv-node--level${Math.min(level, 4)}`}>
      {/* Đường nối dọc từ cha */}
      {level > 0 && (
        <div 
          className={`ltv-node__connector ${isLast ? 'ltv-node__connector--last' : ''}`}
          style={{ borderColor: getLevelColor(level - 1).border }}
        />
      )}

      {/* Card chính */}
      <Card 
        className="ltv-card"
        style={{ 
          borderLeftColor: levelColor.border,
          backgroundColor: levelColor.bg
        }}
      >
        <Card.Body className="ltv-card__body">
          {/* Row 1: Main content */}
          <div className="ltv-card__main" onClick={handleToggle}>
            {/* Expand icon */}
            <div className="ltv-card__expand">
              {hasChildren ? (
                <span className="ltv-expand-icon ltv-expand-icon--active">
                  {expanded ? <FaChevronDown /> : <FaChevronRight />}
                </span>
              ) : (
                <span className="ltv-expand-icon ltv-expand-icon--placeholder" />
              )}
            </div>

            {/* Icon theo type */}
            <div className={`ltv-card__type-icon ltv-card__type-icon--${getLectureBadgeVariant(type)}`}>
              {getLectureIcon(type)}
            </div>

            {/* Info */}
            <div className="ltv-card__info">
              {/* Title row */}
              <div className="ltv-card__title-row">
                {numberingLabel && (
                  <span className="ltv-card__numbering">{numberingLabel}</span>
                )}
                <h6 className="ltv-card__title" onClick={handleView} title="Nhấn để xem chi tiết">
                  {title}
                  <FaEye className="ltv-card__view-icon" />
                </h6>
              </div>

              {/* Meta row */}
              <div className="ltv-card__meta">
                <Badge bg={getLectureBadgeVariant(type)} className="ltv-badge ltv-badge--type">
                  {getLectureLabel(type)}
                </Badge>

                {/* Level indicator */}
                <span 
                  className="ltv-badge ltv-badge--level"
                  style={{ backgroundColor: levelColor.border, color: '#fff' }}
                >
                  {level === 0 ? 'Gốc' : `Cấp ${level}`}
                </span>

                {/* Children count */}
                {hasChildren && (
                  <Badge bg="secondary" className="ltv-badge ltv-badge--children">
                    <FaSitemap className="me-1" style={{ fontSize: '0.65rem' }} />
                    {lecture.children.length} con
                  </Badge>
                )}

                {/* Duration for video */}
                {duration && (
                  <span className="ltv-card__duration">
                    <FaClock /> {formatDuration(duration)}
                  </span>
                )}

                {/* Parent info - hiển thị rõ thuộc lecture nào */}
                {parentTitle && (
                  <span className="ltv-card__parent">
                    ↳ Con của: <strong>{parentTitle}</strong>
                  </span>
                )}
              </div>
            </div>

            {/* Actions */}
            <div className="ltv-card__actions" onClick={(e) => e.stopPropagation()}>
              <OverlayTrigger placement="top" overlay={<Tooltip>Thêm bài con vào "{title}"</Tooltip>}>
                <Button variant="success" size="sm" className="ltv-btn ltv-btn--add" onClick={handleAddChild}>
                  <FaPlus />
                  <span className="ltv-btn__text">Thêm con</span>
                </Button>
              </OverlayTrigger>

              <OverlayTrigger placement="top" overlay={<Tooltip>Chỉnh sửa</Tooltip>}>
                <Button variant="outline-primary" size="sm" className="ltv-btn ltv-btn--edit" onClick={handleEdit}>
                  <FaEdit />
                </Button>
              </OverlayTrigger>

              <OverlayTrigger placement="top" overlay={<Tooltip>Xóa</Tooltip>}>
                <Button variant="outline-danger" size="sm" className="ltv-btn ltv-btn--delete" onClick={handleDelete}>
                  <FaTrash />
                </Button>
              </OverlayTrigger>
            </div>
          </div>
        </Card.Body>
      </Card>

      {/* Children */}
      {hasChildren && (
        <Collapse in={expanded}>
          <div className="ltv-children">
            {lecture.children.map((child, index) => (
              <LectureTreeNode
                key={child.lectureId || child.LectureId}
                lecture={child}
                level={level + 1}
                isLast={index === lecture.children.length - 1}
                parentTitle={title}
                onAddChild={onAddChild}
                onEdit={onEdit}
                onDelete={onDelete}
                onView={onView}
                getLectureIcon={getLectureIcon}
                getLectureLabel={getLectureLabel}
                getLectureBadgeVariant={getLectureBadgeVariant}
                getLevelColor={getLevelColor}
              />
            ))}
          </div>
        </Collapse>
      )}
    </div>
  );
}

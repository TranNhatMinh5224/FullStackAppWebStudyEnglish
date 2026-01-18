import React from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import "./MarkdownViewer.css";

function MarkdownViewer({ lecture }) {
    const title = lecture?.title || lecture?.Title || "";
    const markdownContent = lecture?.markdownContent || lecture?.MarkdownContent || "";

    return (
        <div className="markdown-viewer">
            <header className="lecture-header">
                <h1 className="lecture-title">{title}</h1>
            </header>
            <div className="lecture-content">
                {markdownContent && markdownContent.trim().length > 0 ? (
                    <div className="markdown-content">
                        <ReactMarkdown remarkPlugins={[remarkGfm]}>
                            {markdownContent}
                        </ReactMarkdown>
                    </div>
                ) : (
                    <div className="no-content-message">
                        <p>Nội dung bài giảng đang được cập nhật...</p>
                    </div>
                )}
            </div>
        </div>
    );
}

MarkdownViewer.displayName = "MarkdownViewer";

export default MarkdownViewer;

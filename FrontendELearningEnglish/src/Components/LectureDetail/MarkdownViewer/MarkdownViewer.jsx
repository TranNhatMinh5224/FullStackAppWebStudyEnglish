import React from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import "./MarkdownViewer.css";

export default function MarkdownViewer({ lecture }) {
    const title = lecture?.title || lecture?.Title || "";
    const markdownContent = lecture?.markdownContent || lecture?.MarkdownContent || "";
    const renderedHtml = lecture?.renderedHtml || lecture?.RenderedHtml || "";

    // Prefer markdownContent for better rendering with react-markdown
    // Only use renderedHtml if markdownContent is not available
    const useMarkdown = markdownContent && markdownContent.trim().length > 0;

    return (
        <div className="markdown-viewer">
            <header className="lecture-header">
                <h1 className="lecture-title">{title}</h1>
            </header>
            <div className="lecture-content">
                {useMarkdown ? (
                    <div className="markdown-content">
                        <ReactMarkdown 
                            remarkPlugins={[remarkGfm]}
                        >
                            {markdownContent}
                        </ReactMarkdown>
                    </div>
                ) : renderedHtml ? (
                    <div 
                        className="markdown-html-content"
                        dangerouslySetInnerHTML={{ __html: renderedHtml }}
                    />
                ) : (
                    <div className="no-content-message">
                        <p>Nội dung bài giảng đang được cập nhật...</p>
                    </div>
                )}
            </div>
        </div>
    );
}


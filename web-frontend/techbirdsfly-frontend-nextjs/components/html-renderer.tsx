"use client";

import React from "react";

interface HtmlRendererProps {
  html: string;
  className?: string;
}

export default function HtmlRenderer({ html, className = "" }: HtmlRendererProps) {
  return (
    <div
      className={`bg-white dark:bg-neutral-900 rounded-lg border border-gray-200 dark:border-neutral-800 overflow-hidden shadow-sm ${className}`}
      dangerouslySetInnerHTML={{ __html: html }}
      style={{
        WebkitFontSmoothing: "antialiased",
        MozOsxFontSmoothing: "grayscale",
      }}
    />
  );
}

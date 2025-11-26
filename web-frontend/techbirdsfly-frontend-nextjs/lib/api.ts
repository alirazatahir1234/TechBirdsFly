import { GenerateWebsitePayload, ApiResponse, GeneratedWebsiteDto } from "./types";

// Gateway URL for all API calls
const GATEWAY_URL = process.env.NEXT_PUBLIC_GATEWAY_URL || "http://localhost:5500";
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5500";

export async function generateWebsite(
  payload: GenerateWebsitePayload
): Promise<ApiResponse<GeneratedWebsiteDto>> {
  try {
    // Route through gateway: /api/generator/** → Generator Service
    const response = await fetch(`${API_BASE_URL}/api/generator/api/v1/generate`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
      cache: "no-store",
    });

    if (!response.ok) {
      const error = await response.json();
      return {
        success: false,
        message: error.message || "Failed to generate website",
        errors: error.errors,
      };
    }

    const data = await response.json();
    return data;
  } catch (error) {
    return {
      success: false,
      message: error instanceof Error ? error.message : "An error occurred",
    };
  }
}

export async function getHealthStatus(): Promise<boolean> {
  try {
    // Health check endpoint on the gateway
    const response = await fetch(`${GATEWAY_URL}/health`, {
      method: "GET",
      cache: "no-store",
    });
    return response.ok;
  } catch (error) {
    return false;
  }
}

export async function exportAsHtml(html: string, filename: string): Promise<void> {
  const element = document.createElement("a");
  element.setAttribute("href", "data:text/html;charset=utf-8," + encodeURIComponent(html));
  element.setAttribute("download", filename);
  element.style.display = "none";
  document.body.appendChild(element);
  element.click();
  document.body.removeChild(element);
}

export async function exportAsZip(
  html: string,
  css: string,
  js: string,
  filename: string
): Promise<void> {
  // Simplified zip export - for production, use a library like jszip
  const projectContent = `
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Generated Website</title>
    <style>
${css}
    </style>
</head>
<body>
${html}
    <script>
${js}
    </script>
</body>
</html>
  `;

  exportAsHtml(projectContent, filename);
}

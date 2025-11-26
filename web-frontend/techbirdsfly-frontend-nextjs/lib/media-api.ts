/**
 * Media-Service API Client
 * Handles image upload and AI-powered image generation
 * 
 * Gateway format: http://localhost:5500/media → Media-Service on 9000
 */

/**
 * Upload an image file to Media-Service
 * @param file - File object from input
 * @returns Promise with { id, url, base64 }
 */
export async function uploadImage(file: File): Promise<any> {
  try {
    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch("http://localhost:9000/media/api/media/upload", {
      method: "POST",
      body: formData,
    });

    if (!response.ok) {
      throw new Error(`Upload failed: ${response.statusText}`);
    }

    const data = await response.json();
    return {
      id: data.id || data.mediaId,
      url: data.url || data.mediaUrl,
      base64: data.base64,
      size: data.size,
      mimeType: data.mimeType,
      uploadedAt: data.uploadedAt || new Date().toISOString(),
    };
  } catch (error) {
    console.error("[Media-Service] Upload error:", error);
    throw new Error(
      `Failed to upload image: ${error instanceof Error ? error.message : "Unknown error"}`
    );
  }
}

/**
 * Generate an AI image from a text prompt
 * @param prompt - Text description of image to generate
 * @returns Promise with { base64, url, promptUsed }
 */
export async function generateAIImage(prompt: string): Promise<any> {
  try {
    if (!prompt.trim()) {
      throw new Error("Prompt cannot be empty");
    }

    const response = await fetch(
      "http://localhost:9000/media/api/media/generate",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt }),
      }
    );

    if (!response.ok) {
      throw new Error(`Generation failed: ${response.statusText}`);
    }

    const data = await response.json();
    return {
      base64: data.base64,
      url: data.url || data.mediaUrl,
      promptUsed: prompt,
      generatedAt: data.generatedAt || new Date().toISOString(),
      id: data.id || data.mediaId,
    };
  } catch (error) {
    console.error("[Media-Service] Generation error:", error);
    throw new Error(
      `Failed to generate image: ${error instanceof Error ? error.message : "Unknown error"}`
    );
  }
}

/**
 * Get media item details
 * @param mediaId - ID of media item
 * @returns Promise with media metadata
 */
export async function getMediaItem(mediaId: string): Promise<any> {
  try {
    const response = await fetch(
      `http://localhost:9000/media/api/media/${mediaId}`,
      {
        method: "GET",
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch media: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    console.error("[Media-Service] Get media error:", error);
    throw new Error(
      `Failed to fetch media: ${error instanceof Error ? error.message : "Unknown error"}`
    );
  }
}

/**
 * Delete a media item
 * @param mediaId - ID of media to delete
 */
export async function deleteMedia(mediaId: string): Promise<void> {
  try {
    const response = await fetch(
      `http://localhost:9000/media/api/media/${mediaId}`,
      {
        method: "DELETE",
      }
    );

    if (!response.ok) {
      throw new Error(`Delete failed: ${response.statusText}`);
    }
  } catch (error) {
    console.error("[Media-Service] Delete error:", error);
    throw new Error(
      `Failed to delete media: ${error instanceof Error ? error.message : "Unknown error"}`
    );
  }
}

/**
 * List all media items for user
 * @returns Promise with array of media items
 */
export async function listMedia(): Promise<any[]> {
  try {
    const response = await fetch(
      "http://localhost:9000/media/api/media/list",
      {
        method: "GET",
      }
    );

    if (!response.ok) {
      throw new Error(`List failed: ${response.statusText}`);
    }

    const data = await response.json();
    return Array.isArray(data) ? data : data.media || [];
  } catch (error) {
    console.error("[Media-Service] List error:", error);
    throw new Error(
      `Failed to list media: ${error instanceof Error ? error.message : "Unknown error"}`
    );
  }
}

import { NextRequest, NextResponse } from "next/server";

/**
 * API Proxy Route for Generator Microservice
 *
 * This route automatically forwards ANY request to the .NET Generator Service
 * running behind YARP (Yet Another Reverse Proxy).
 *
 * Supports: GET, POST, PUT, DELETE
 * Maps: /api/generator/* → {NEXT_PUBLIC_GATEWAY_URL}/generator/api/*
 *
 * Examples:
 * - POST /api/generator/projects → .NET Service
 * - GET /api/generator/projects/{id} → .NET Service
 * - PUT /api/generator/projects/{id} → .NET Service
 * - DELETE /api/generator/projects/{id} → .NET Service
 */

export async function GET(req: NextRequest, { params }: any) {
  const resolvedParams = await params;
  return handleProxy("GET", req, resolvedParams);
}

export async function POST(req: NextRequest, { params }: any) {
  const resolvedParams = await params;
  return handleProxy("POST", req, resolvedParams);
}

export async function PUT(req: NextRequest, { params }: any) {
  const resolvedParams = await params;
  return handleProxy("PUT", req, resolvedParams);
}

export async function DELETE(req: NextRequest, { params }: any) {
  const resolvedParams = await params;
  return handleProxy("DELETE", req, resolvedParams);
}

async function handleProxy(
  method: string,
  req: NextRequest,
  params: any
): Promise<NextResponse> {
  try {
    // Extract the endpoint from params
    const endpoint = params.endpoint.join("/");
    const gatewayUrl = process.env.NEXT_PUBLIC_GATEWAY_URL;
    const generatorRoute = process.env.GENERATOR_ROUTE || "/generator/api";

    if (!gatewayUrl) {
      console.error("NEXT_PUBLIC_GATEWAY_URL not configured");
      return NextResponse.json(
        { error: "Gateway not configured" },
        { status: 500 }
      );
    }

    // Construct full URL
    const url = `${gatewayUrl}${generatorRoute}/${endpoint}`;
    console.log(`[Generator API Proxy] ${method} ${url}`);

    // Get request body for POST/PUT
    let body: string | undefined;
    if (method !== "GET" && method !== "DELETE") {
      try {
        body = await req.text();
      } catch (e) {
        console.error("Failed to read request body:", e);
        body = undefined;
      }
    }

    // Get auth token from session/cookies
    const token = req.headers.get("authorization");
    const userId = req.headers.get("x-user-id");

    // Forward request to .NET service
    const response = await fetch(url, {
      method,
      headers: {
        "Content-Type": "application/json",
        ...(token && { Authorization: token }),
        ...(userId && { "X-User-Id": userId }),
      },
      body: body,
    });

    // Read response
    const responseText = await response.text();

    // Try to parse as JSON, fallback to plain text
    let responseData;
    try {
      responseData = JSON.parse(responseText);
    } catch {
      responseData = responseText;
    }

    // Log response status
    console.log(
      `[Generator API Proxy] ${method} ${url} - Status: ${response.status}`
    );

    // Return response with appropriate headers
    return new NextResponse(JSON.stringify(responseData), {
      status: response.status,
      headers: {
        "Content-Type":
          response.headers.get("Content-Type") || "application/json",
        "Cache-Control": "no-store",
      },
    });
  } catch (error) {
    console.error("[Generator API Proxy] Error:", error);
    return NextResponse.json(
      {
        error: error instanceof Error ? error.message : "Proxy request failed",
      },
      { status: 500 }
    );
  }
}

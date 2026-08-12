import { NextResponse } from "next/server";

const backendUrl = process.env.LOANS_API_URL ?? "http://localhost:5080";

/**
 * Forwards the form to the .NET API from the server side.
 *
 * The browser never talks to the backend directly. That keeps the SSN off any
 * cross-origin request, leaves the API's address a server-side detail, and means
 * there is no CORS configuration to get wrong. It is a proxy and nothing more:
 * no decision is made or interpreted here.
 */
export async function POST(request: Request) {
  const body = await request.text();

  try {
    const response = await fetch(`${backendUrl}/api/loan-applications`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body,
      cache: "no-store",
    });

    return new NextResponse(await response.text(), {
      status: response.status,
      headers: { "Content-Type": response.headers.get("Content-Type") ?? "application/json" },
    });
  } catch {
    // The API is not running, or refused the connection.
    return NextResponse.json(
      { title: "The application service is unavailable. Please try again in a moment." },
      { status: 503 },
    );
  }
}

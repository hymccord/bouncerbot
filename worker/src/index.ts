export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    try {
      const url = new URL(request.url);

      const forwardUrl = new URL(
        "https://www.mousehuntgame.com" + url.pathname
      );

      // Copy query parameters
      url.searchParams.forEach((value, key) => {
        forwardUrl.searchParams.append(key, value);
      });

      // Buffer the request body to avoid redirect issues
      let body: ArrayBuffer | null = null;
      if (
        request.body &&
        (request.method === "POST" ||
          request.method === "PUT" ||
          request.method === "PATCH")
      ) {
        body = await request.arrayBuffer();
      }

      // Create the forwarding request with buffered body
      const forwardRequest = new Request(forwardUrl.toString(), {
        method: request.method,
        headers: request.headers,
        body: body,
      });

      // Forward the request
      const response = await fetch(forwardRequest);

      // See if body is valid json and set content type accordingly
      let validJson = true;
      try {
        await response.clone().json();
      } catch (error) {
        validJson = false;
      }

      // Create a new response to avoid immutable response issues
      const forwardedResponse = new Response(response.body, {
        status: response.status,
        statusText: response.statusText,
        headers: response.headers,
      });

      if (validJson) {
        forwardedResponse.headers.set("Content-Type", "application/json");
      }

      return forwardedResponse;
    } catch (error) {
      console.error("Error forwarding request:", error);
      return new Response("Internal Server Error", { status: 500 });
    }
  },
} satisfies ExportedHandler<Env>;

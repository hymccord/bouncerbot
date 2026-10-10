import type { RequestOutcome } from "./telemetry";

export const RELAY_KEY_HEADER = "X-BouncerBot-Relay-Key";

const UPSTREAM_ORIGIN = "https://www.mousehuntgame.com";
const USER_AGENT = "BouncerBot-Relay/1.0 (Discord: Xellis)";
const CLIENT_NAME = "bouncerbot-relay";

export interface RelayOptions {
  /** Requests must carry a matching {@link RELAY_KEY_HEADER}. */
  relayKey: string;
}

export interface RelayResult {
  response: Response;
  outcome: RequestOutcome;
  upstreamStatus?: number;
  upstreamMs?: number;
  rayId?: string | null;
  cfMitigated?: string | null;
  error?: unknown;
}

export async function handleRelayRequest(
  request: Request,
  options: RelayOptions,
): Promise<RelayResult> {
  if (!hasValidRelayKey(request, options.relayKey)) {
    return {
      outcome: "unauthorized",
      response: jsonResponse(401, {
        error: "relay_unauthorized",
        message: `Missing or invalid ${RELAY_KEY_HEADER} header.`,
      }),
    };
  }

  try {
    const url = new URL(request.url);
    const forwardUrl = new URL(UPSTREAM_ORIGIN + url.pathname);

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

    const headers = new Headers();
    for (const [name, value] of request.headers) {
      if (shouldForwardRequestHeader(name)) {
        headers.set(name, value);
      }
    }

    headers.set("User-Agent", USER_AGENT);
    headers.set("X-BouncerBot-Client", CLIENT_NAME);

    const forwardRequest = new Request(forwardUrl.toString(), {
      method: request.method,
      headers,
      body: body,
    });

    // Forward the request
    const started = performance.now();
    const response = await fetch(forwardRequest);
    const upstream = {
      upstreamStatus: response.status,
      upstreamMs: Math.round(performance.now() - started),
      rayId: response.headers.get("cf-ray"),
      cfMitigated: response.headers.get("cf-mitigated"),
    };

    if (await isCloudflareChallenge(response)) {
      const challengeHeaders = new Headers({
        "Cache-Control": "no-store",
        "Content-Type": "application/json",
        "X-Upstream-Status": response.status.toString(),
      });
      copyResponseHeader(response.headers, challengeHeaders, "cf-ray");
      copyResponseHeader(response.headers, challengeHeaders, "cf-mitigated");

      return {
        ...upstream,
        outcome: "challenge",
        response: new Response(
          JSON.stringify({
            error: "upstream_cloudflare_challenge",
            message:
              "The upstream returned a Cloudflare managed challenge. Configure an authorized service/API rule or inspect the request with the owner-controlled browser diagnostic.",
            upstreamStatus: response.status,
            rayId: response.headers.get("cf-ray"),
          }),
          {
            status: 502,
            headers: challengeHeaders,
          },
        ),
      };
    }

    // fetch() has already decoded the body, so the upstream encoding and
    // length no longer describe what we send back.
    const responseHeaders = new Headers(response.headers);
    responseHeaders.delete("content-encoding");
    responseHeaders.delete("content-length");
    for (const name of hopByHopHeaders) {
      responseHeaders.delete(name);
    }

    // MouseHunt labels JSON as text/javascript; the bot only parses error
    // bodies typed as application/json.
    if (await isJsonBody(response)) {
      responseHeaders.set("Content-Type", "application/json");
    }

    return {
      ...upstream,
      outcome: "ok",
      response: new Response(response.body, {
        status: response.status,
        statusText: response.statusText,
        headers: responseHeaders,
      }),
    };
  } catch (error) {
    return {
      outcome: "exception",
      error,
      response: new Response("Internal Server Error", { status: 500 }),
    };
  }
}

const hopByHopHeaders = new Set([
  "connection",
  "keep-alive",
  "proxy-authenticate",
  "proxy-authorization",
  "te",
  "trailer",
  "transfer-encoding",
  "upgrade",
]);

// Headers added by the reverse proxy in front of the relay (or by the client)
// that would leak the caller's address or confuse the upstream.
const strippedRequestHeaders = new Set([
  "host",
  "forwarded",
  "x-real-ip",
  "via",
  // Let the runtime negotiate compression so it can always decode the body.
  "accept-encoding",
  RELAY_KEY_HEADER.toLowerCase(),
]);

function shouldForwardRequestHeader(name: string): boolean {
  const lower = name.toLowerCase();
  return (
    !hopByHopHeaders.has(lower) &&
    !strippedRequestHeaders.has(lower) &&
    !lower.startsWith("cf-") &&
    !lower.startsWith("x-forwarded-")
  );
}

function hasValidRelayKey(request: Request, relayKey: string): boolean {
  const provided = request.headers.get(RELAY_KEY_HEADER);
  return provided !== null && timingSafeEqual(provided, relayKey);
}

function timingSafeEqual(actual: string, expected: string): boolean {
  const encoder = new TextEncoder();
  const a = encoder.encode(actual);
  const b = encoder.encode(expected);

  let diff = a.length ^ b.length;
  for (let i = 0; i < b.length; i++) {
    diff |= (a[i] ?? 0) ^ b[i];
  }
  return diff === 0;
}

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: {
      "Cache-Control": "no-store",
      "Content-Type": "application/json",
    },
  });
}

function copyResponseHeader(
  source: Headers,
  target: Headers,
  name: string,
): void {
  const value = source.get(name);
  if (value !== null) {
    target.set(name, value);
  }
}

async function isJsonBody(response: Response): Promise<boolean> {
  try {
    JSON.parse(await response.clone().text());
    return true;
  } catch {
    return false;
  }
}

async function isCloudflareChallenge(response: Response): Promise<boolean> {
  if (response.headers.get("cf-mitigated") === "challenge") {
    return true;
  }

  const contentType = response.headers.get("content-type")?.toLowerCase() ?? "";
  if (!contentType.includes("text/html")) {
    return false;
  }

  const body = await response.clone().text();
  return (
    body.includes("Just a moment...") ||
    body.includes("challenge-platform") ||
    body.includes("cf_chl_opt")
  );
}

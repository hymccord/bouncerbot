import { afterEach, describe, expect, it, vi } from "vitest";

import { handleRelayRequest, RELAY_KEY_HEADER } from "../src/handler";

const options = { relayKey: "secret-key" };

function relayRequest(path: string, init: RequestInit = {}): Request {
  const headers = new Headers(init.headers);
  if (!headers.has(RELAY_KEY_HEADER)) {
    headers.set(RELAY_KEY_HEADER, options.relayKey);
  }
  return new Request(`https://relay.example${path}`, { ...init, headers });
}

function mockUpstream(handler: (request: Request) => Response | Promise<Response>) {
  const upstream = vi.fn(async (request: Request) => handler(request));
  globalThis.fetch = upstream as unknown as typeof globalThis.fetch;
  return upstream;
}

function jsonUpstream() {
  return mockUpstream(
    () =>
      new Response('{"ok":true}', {
        status: 200,
        headers: { "Content-Type": "application/json", "Cf-Ray": "ok-ray" },
      }),
  );
}

describe("relay handler", () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
    vi.restoreAllMocks();
  });

  it("preserves the request while filtering hop-by-hop, Cloudflare and proxy headers", async () => {
    const upstream = mockUpstream(async (request) => {
      expect(request.url).toBe("https://www.mousehuntgame.com/api/get/user/me?test=1");
      expect(request.method).toBe("POST");
      expect(await request.text()).toBe("account_name=test");

      expect(request.headers.get("User-Agent")).toBe("BouncerBot-Relay/1.0 (Discord: Xellis)");
      expect(request.headers.get("X-BouncerBot-Client")).toBe("bouncerbot-relay");
      expect(request.headers.get("Cookie")).toBe("HG_TOKEN=abc");

      for (const name of [
        RELAY_KEY_HEADER,
        "Connection",
        "Cf-Connecting-Ip",
        "X-Forwarded-For",
        "X-Forwarded-Proto",
        "X-Forwarded-Host",
        "X-Real-IP",
        "Forwarded",
        "Via",
        "Accept-Encoding",
      ]) {
        expect(request.headers.get(name), name).toBeNull();
      }

      return new Response('{"ok":true}', {
        status: 200,
        headers: { "Content-Type": "application/json", "Cf-Ray": "ok-ray" },
      });
    });

    const result = await handleRelayRequest(
      relayRequest("/api/get/user/me?test=1", {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          Connection: "keep-alive",
          "Cf-Connecting-Ip": "203.0.113.10",
          "X-Forwarded-For": "198.51.100.7",
          "X-Forwarded-Proto": "https",
          "X-Forwarded-Host": "relay.example",
          "X-Real-IP": "198.51.100.7",
          Forwarded: "for=198.51.100.7",
          Via: "1.1 Caddy",
          "Accept-Encoding": "gzip, br",
          Cookie: "HG_TOKEN=abc",
        },
        body: "account_name=test",
      }),
      options,
    );

    expect(upstream).toHaveBeenCalledOnce();
    expect(result).toMatchObject({ outcome: "ok", upstreamStatus: 200, rayId: "ok-ray" });
    expect(result.upstreamMs).toBeGreaterThanOrEqual(0);
    expect(result.response.status).toBe(200);
    expect(await result.response.json()).toEqual({ ok: true });
  });

  it.each([
    ["missing", null],
    ["wrong", "not-the-key"],
    ["prefix of", "secret"],
  ])("rejects a %s relay key without contacting upstream", async (_, key) => {
    const upstream = jsonUpstream();

    const headers = new Headers();
    if (key !== null) {
      headers.set(RELAY_KEY_HEADER, key);
    }

    const result = await handleRelayRequest(
      new Request("https://relay.example/api/get/user/me", { headers }),
      options,
    );

    expect(result.outcome).toBe("unauthorized");
    expect(result.response.status).toBe(401);
    expect(result.response.headers.get("Cache-Control")).toBe("no-store");
    expect(await result.response.json()).toMatchObject({ error: "relay_unauthorized" });
    expect(upstream).not.toHaveBeenCalled();
  });

  it("normalizes a managed Cloudflare challenge without caching it", async () => {
    mockUpstream(
      () =>
        new Response("<html><title>Just a moment...</title></html>", {
          status: 403,
          headers: { "Content-Type": "text/html", "Cf-Ray": "test-ray", "Cf-Mitigated": "challenge" },
        }),
    );

    const result = await handleRelayRequest(relayRequest("/api/get/user/me"), options);

    expect(result).toMatchObject({
      outcome: "challenge",
      upstreamStatus: 403,
      rayId: "test-ray",
      cfMitigated: "challenge",
    });
    expect(result.response.status).toBe(502);
    expect(result.response.headers.get("Cache-Control")).toBe("no-store");
    expect(result.response.headers.get("Cf-Ray")).toBe("test-ray");
    expect(await result.response.json()).toMatchObject({
      error: "upstream_cloudflare_challenge",
      upstreamStatus: 403,
      rayId: "test-ray",
    });
  });

  it("removes upstream encoding and length headers from the response", async () => {
    mockUpstream(
      () =>
        new Response('{"ok":true}', {
          status: 200,
          headers: {
            "Content-Type": "application/json",
            "Content-Encoding": "br",
            "Content-Length": "999",
            "Set-Cookie": "HG_TOKEN=abc; Path=/",
          },
        }),
    );

    const { response } = await handleRelayRequest(relayRequest("/api/get/user/me"), options);

    expect(response.headers.get("Content-Encoding")).toBeNull();
    expect(response.headers.get("Content-Length")).toBeNull();
    expect(response.headers.get("Set-Cookie")).toBe("HG_TOKEN=abc; Path=/");
    expect(await response.json()).toEqual({ ok: true });
  });

  it.each([
    ['{"error":{"code":100}}', "text/javascript;charset=UTF-8", "application/json"],
    ["<html>Oops</html>", "text/html", "text/html"],
  ])("labels JSON bodies as application/json (%s)", async (body, upstreamType, expectedType) => {
    mockUpstream(
      () => new Response(body, { status: 400, headers: { "Content-Type": upstreamType } }),
    );

    const { response } = await handleRelayRequest(relayRequest("/api/get/user/me"), options);

    expect(response.status).toBe(400);
    expect(response.headers.get("Content-Type")).toBe(expectedType);
    expect(await response.text()).toBe(body);
  });

  it("reports an upstream failure as an exception with a 500", async () => {
    const failure = new TypeError("connection refused");
    mockUpstream(() => {
      throw failure;
    });

    const result = await handleRelayRequest(relayRequest("/api/get/user/me"), options);

    expect(result.outcome).toBe("exception");
    expect(result.error).toBe(failure);
    expect(result.response.status).toBe(500);
  });
});

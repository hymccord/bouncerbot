import { afterEach, describe, expect, it, vi } from "vitest";

import { initTelemetry, logRequest, type RequestLogFields } from "../src/telemetry";

const baseFields: RequestLogFields = {
  method: "POST",
  path: "/api/get/user/me",
  query: "",
  status: 200,
  outcome: "ok",
  durationMs: 42,
  upstreamStatus: 200,
  upstreamMs: 40,
  rayId: "abc123-ATL",
  clientAddress: "198.51.100.7",
  userAgent: "BouncerBot/1.0",
};

function captureLines() {
  const spy = vi.spyOn(console, "log").mockImplementation(() => {});
  return () => spy.mock.calls.map(([line]) => JSON.parse(line as string));
}

describe("telemetry", () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("does not enable OTLP without an endpoint", () => {
    expect(initTelemetry({})).toBe(false);
  });

  it("writes one JSON line per request with Workers-style attributes", () => {
    const lines = captureLines();

    logRequest(baseFields);

    const records = lines();
    expect(records).toHaveLength(1);
    expect(records[0]).toMatchObject({
      level: "info",
      msg: "POST /api/get/user/me 200",
      "http.request.method": "POST",
      "url.path": "/api/get/user/me",
      "http.response.status_code": 200,
      outcome: "ok",
      duration_ms: 42,
      "upstream.status_code": 200,
      "upstream.duration_ms": 40,
      "cf.ray": "abc123-ATL",
      "client.address": "198.51.100.7",
      "user_agent.original": "BouncerBot/1.0",
    });
    expect(typeof records[0].time).toBe("string");
    // Empty values are dropped rather than logged as blanks.
    expect(records[0]).not.toHaveProperty("url.query");
    expect(records[0]).not.toHaveProperty("cf.mitigated");
  });

  it.each([
    ["unauthorized", "warn"],
    ["challenge", "warn"],
    ["exception", "error"],
  ] as const)("logs %s requests at %s", (outcome, level) => {
    const lines = captureLines();

    logRequest({ ...baseFields, outcome, status: 502 });

    expect(lines()[0].level).toBe(level);
  });

  it("includes exception details", () => {
    const lines = captureLines();

    logRequest({ ...baseFields, outcome: "exception", status: 500, error: new TypeError("boom") });

    expect(lines()[0]).toMatchObject({
      level: "error",
      "exception.type": "TypeError",
      "exception.message": "boom",
      "exception.stacktrace": expect.stringContaining("boom"),
    });
  });
});

// Relay entry point, run in Docker on a residential connection. See deploy/relay/.
import { handleRelayRequest } from "./handler";
import { initTelemetry, logEvent, logRequest, shutdownTelemetry } from "./telemetry";

const otlpEnabled = initTelemetry();

const relayKey = Bun.env.RELAY_KEY;
if (!relayKey) {
  logEvent("error", "RELAY_KEY must be set; refusing to start an unauthenticated relay.");
  await shutdownTelemetry();
  process.exit(1);
}

const port = Number(Bun.env.PORT ?? 8080);

const server = Bun.serve({
  port,
  // MouseHunt can be slow; don't drop the client connection while waiting on it.
  idleTimeout: 60,
  async fetch(request) {
    const url = new URL(request.url);
    if (url.pathname === "/_relay/health") {
      return new Response("ok");
    }

    const started = performance.now();
    const result = await handleRelayRequest(request, { relayKey });

    logRequest({
      method: request.method,
      path: url.pathname,
      query: url.search.slice(1),
      status: result.response.status,
      outcome: result.outcome,
      durationMs: Math.round(performance.now() - started),
      upstreamStatus: result.upstreamStatus,
      upstreamMs: result.upstreamMs,
      rayId: result.rayId,
      cfMitigated: result.cfMitigated,
      // Caddy puts the real client first in X-Forwarded-For.
      clientAddress:
        request.headers.get("x-forwarded-for")?.split(",")[0]?.trim() ??
        server.requestIP(request)?.address,
      userAgent: request.headers.get("user-agent"),
      error: result.error,
    });

    return result.response;
  },
});

logEvent("info", `BouncerBot relay listening on :${server.port}`, { otlp: otlpEnabled });

let stopping = false;
async function stop(signal: string) {
  if (stopping) {
    return;
  }
  stopping = true;

  logEvent("info", `Received ${signal}, shutting down`);
  await server.stop();
  await shutdownTelemetry();
  process.exit(0);
}

process.on("SIGTERM", () => void stop("SIGTERM"));
process.on("SIGINT", () => void stop("SIGINT"));

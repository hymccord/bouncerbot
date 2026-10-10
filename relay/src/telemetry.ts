// Structured logging that stands in for Cloudflare Workers Logs: every record
// is written to stdout as a JSON line (`docker logs`), and also exported over
// OTLP (e.g. to Grafana) when OTEL_EXPORTER_OTLP_ENDPOINT is configured.
import { hostname } from "node:os";

import { SeverityNumber, type Logger } from "@opentelemetry/api-logs";
import { OTLPLogExporter } from "@opentelemetry/exporter-logs-otlp-proto";
import { resourceFromAttributes } from "@opentelemetry/resources";
import { BatchLogRecordProcessor, LoggerProvider } from "@opentelemetry/sdk-logs";
import {
  ATTR_SERVICE_INSTANCE_ID,
  ATTR_SERVICE_NAME,
  ATTR_SERVICE_VERSION,
} from "@opentelemetry/semantic-conventions";

export type Severity = "info" | "warn" | "error";
export type Attributes = Record<string, string | number | boolean | null | undefined>;

const severityNumbers: Record<Severity, SeverityNumber> = {
  info: SeverityNumber.INFO,
  warn: SeverityNumber.WARN,
  error: SeverityNumber.ERROR,
};

let provider: LoggerProvider | undefined;
let otelLogger: Logger | undefined;

/** Enables OTLP export when configured. Returns whether OTLP is active. */
export function initTelemetry(env: Record<string, string | undefined> = process.env): boolean {
  if (!env.OTEL_EXPORTER_OTLP_ENDPOINT) {
    return false;
  }

  provider = new LoggerProvider({
    resource: resourceFromAttributes({
      [ATTR_SERVICE_NAME]: env.OTEL_SERVICE_NAME ?? "bouncerbot-relay",
      [ATTR_SERVICE_VERSION]: env.RELAY_VERSION ?? "dev",
      [ATTR_SERVICE_INSTANCE_ID]: hostname(),
    }),
    // The exporter reads OTEL_EXPORTER_OTLP_ENDPOINT/HEADERS itself.
    processors: [new BatchLogRecordProcessor({ exporter: new OTLPLogExporter() })],
  });
  otelLogger = provider.getLogger("bouncerbot-relay");
  return true;
}

/** Flushes buffered OTLP records; call before the process exits. */
export async function shutdownTelemetry(): Promise<void> {
  await provider?.shutdown();
  provider = undefined;
  otelLogger = undefined;
}

export function logEvent(severity: Severity, message: string, attributes: Attributes = {}): void {
  const attrs = withoutEmpty(attributes);

  console.log(
    JSON.stringify({ time: new Date().toISOString(), level: severity, msg: message, ...attrs }),
  );

  otelLogger?.emit({
    severityNumber: severityNumbers[severity],
    severityText: severity.toUpperCase(),
    body: message,
    attributes: attrs,
  });
}

export type RequestOutcome = "ok" | "unauthorized" | "challenge" | "exception";

export interface RequestLogFields {
  method: string;
  path: string;
  query: string;
  status: number;
  outcome: RequestOutcome;
  durationMs: number;
  upstreamStatus?: number;
  upstreamMs?: number;
  rayId?: string | null;
  cfMitigated?: string | null;
  clientAddress?: string | null;
  userAgent?: string | null;
  error?: unknown;
}

const outcomeSeverity: Record<RequestOutcome, Severity> = {
  ok: "info",
  unauthorized: "warn",
  challenge: "warn",
  exception: "error",
};

/** One record per relayed request, mirroring a Workers invocation log. */
export function logRequest(fields: RequestLogFields): void {
  logEvent(outcomeSeverity[fields.outcome], `${fields.method} ${fields.path} ${fields.status}`, {
    "http.request.method": fields.method,
    "url.path": fields.path,
    "url.query": fields.query,
    "http.response.status_code": fields.status,
    outcome: fields.outcome,
    duration_ms: fields.durationMs,
    "upstream.status_code": fields.upstreamStatus,
    "upstream.duration_ms": fields.upstreamMs,
    "cf.ray": fields.rayId,
    "cf.mitigated": fields.cfMitigated,
    "client.address": fields.clientAddress,
    "user_agent.original": fields.userAgent,
    ...exceptionAttributes(fields.error),
  });
}

function exceptionAttributes(error: unknown): Attributes {
  if (error === undefined) {
    return {};
  }
  if (error instanceof Error) {
    return {
      "exception.type": error.name,
      "exception.message": error.message,
      "exception.stacktrace": error.stack,
    };
  }
  return { "exception.message": String(error) };
}

function withoutEmpty(attributes: Attributes): Record<string, string | number | boolean> {
  const result: Record<string, string | number | boolean> = {};
  for (const [key, value] of Object.entries(attributes)) {
    if (value !== undefined && value !== null && value !== "") {
      result[key] = value;
    }
  }
  return result;
}

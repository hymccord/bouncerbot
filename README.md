# BouncerBot

A multi-component project featuring a C# Discord bot (utilizing [NetCord](https://www.netcord.dev)) and a small relay that forwards its MouseHunt traffic. Developed for the [MouseHunt Community Discord Server](https://discordapp.com/invite/Ya9zEdk) for integration with the HitGrab game [MouseHunt](https://www.mousehuntgame.com/).

## Components

### 1. Discord Bot (`discord-bot/`)

- Built with .NET 10.0
- Uses NetCord for Discord API
- Entity Framework Core for data access
- Modular design: commands, services, gateway handlers

#### Building

1. Install [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
2. Build:

   ```pwsh
   dotnet build
   ```

### 2. MouseHunt relay (`relay/`)

Cloudflare challenges MouseHunt requests that come from datacenter IPs, which
includes the bot's host. The relay is a small [Bun](https://bun.sh/) server that
runs in Docker on a home server, so MouseHunt requests leave from a residential
IP. It sits behind Caddy on a public subdomain and rejects any request without
the shared `X-BouncerBot-Relay-Key` header.

- Forwards requests to `https://www.mousehuntgame.com`, with the bot's normal
  User-Agent. It does not impersonate a browser or solve challenges.
- If Cloudflare returns a managed challenge anyway, it answers with a 502
  `upstream_cloudflare_challenge` JSON error (not cached), which the bot
  reports as a `CloudflareChallengeException`.
- Health check: `GET /_relay/health`
- Image: `ghcr.io/hymccord/bouncerbot-relay`

#### Development

From `relay/`:

```sh
bun install
bun run typecheck
bunx vitest run
RELAY_KEY=test bun run dev   # http://localhost:8080
```

#### Home server

1. Generate a key: `openssl rand -hex 32`
2. In `deploy/relay/`, copy `.env.example` to `.env` and set `RELAY_KEY`
3. Point the external network in `compose.yaml` at your reverse proxy's network,
   then run `docker compose up -d`
4. Add the site block from `Caddyfile.example` with your subdomain and reload
   Caddy. If the domain uses Cloudflare DNS, make the record DNS-only (grey
   cloud), or make sure no challenge rule applies to that hostname.

The GHCR package is private by default. Either make it public, or run
`docker login ghcr.io` on the server with a token that has `read:packages`.

#### Logs

The relay writes one structured record per request:

- method, path and query
- status and outcome (`ok`, `unauthorized`, `challenge` or `exception`)
- total and upstream duration
- Cloudflare Ray ID and `cf-mitigated`
- client IP and User-Agent
- for exceptions, the stack trace

Request bodies and the relay key are never logged. Health checks aren't
logged either.

- **stdout:** `docker compose logs -f bouncerbot-relay`, rotated at
  3 × 10 MB.
- **Grafana (optional):** set `OTEL_EXPORTER_OTLP_ENDPOINT` and
  `OTEL_EXPORTER_OTLP_HEADERS` in `.env` to the same OTLP endpoint and
  `Authorization` header as the bot's `Grafana` settings. Then search in
  Explore → Loki with `{service_name="bouncerbot-relay"}`.

#### Bot configuration

Set these on the bot host, for example as systemd environment variables:

```sh
BouncerBotOptions__MouseHuntUrl=https://mh-relay.example.com
BouncerBotOptions__MouseHuntRelayKey=<RELAY_KEY>
```

## CI/CD

- `.github/workflows/dotnet.yml`: builds and tests the bot, and deploys it to
  the droplet on version tags
- `.github/workflows/relay.yml`: typechecks and tests the relay, then pushes
  its image to GHCR on changes to `relay/`

## Testing

- C# bot: MSTest (see `discord-bot/tests/`)
- Relay: Vitest (see `relay/tests/`)

## Contributing

1. Fork the repo
2. Create a feature branch
3. Submit a pull request

## License

MIT

---

For more details, see the documentation in each subdirectory.

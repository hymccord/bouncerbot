# BouncerBot

A multi-component project featuring a C# Discord bot (utilizing [NetCord](https://www.netcord.dev)) and a Cloudflare Worker backend. Developed for the [MouseHunt Community Discord Server](https://discordapp.com/invite/Ya9zEdk) for integration with the HitGrab game [MouseHunt](https://www.mousehuntgame.com/).

## Components

### 1. Discord Bot (`bot/`)

- Built with .NET 9.0
- Uses NetCord for Discord API
- Entity Framework Core for data access
- Modular design: commands, services, gateway handlers

#### Setup

1. Install [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
2. Navigate to `bot/src/BouncerBot`
3. Restore dependencies:

   ```pwsh
   dotnet restore
   ```

4. Build and run:

   ```pwsh
   dotnet run
   ```

### 2. Cloudflare Worker (`worker/`)

- A simple Request trampoline due to Cloudflare protection for MouseHunt
- Uses Wrangler for deployment

#### Setup

1. Install [Bun](https://bun.sh/) and [Wrangler](https://developers.cloudflare.com/workers/wrangler/)
2. Navigate to `worker`
3. Install dependencies:

   ```pwsh
   bun install
   ```

4. Run locally:

   ```pwsh
   wrangler dev
   ```

5. Deploy:

   ```pwsh
   wrangler deploy
   ```

## CI/CD

- Automated deployment via GitHub Actions (`.github/workflows/cloudflare.yml`)
- Deploys worker from `/worker` directory

## Testing

- C# bot: MSTest (see `bot/tests/`)
- Worker: Vitest (see `worker/tests/`)

## Contributing

1. Fork the repo
2. Create a feature branch
3. Submit a pull request

## License

MIT

---

For more details, see the documentation in each subdirectory.

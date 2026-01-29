# Grafana Integration

Graspire includes built-in support for replacing the Aspire dashboard's telemetry (logs, traces, metrics) with a full Grafana observability stack powered by [grafana/otel-lgtm](https://github.com/grafana/docker-otel-lgtm).

## What it does

- Adds a `grafana/otel-lgtm` container (Grafana + Loki + Tempo + Mimir + OpenTelemetry Collector)
- Redirects all OTLP telemetry from your services to Grafana instead of the Aspire dashboard's built-in receiver
- Adds a **Grafana** tab to the Aspire dashboard that embeds the Grafana UI
- Keyboard shortcut: press `g` to navigate to Grafana

## Usage

In your AppHost `Program.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddGrafana();

// Add your services as usual
builder.AddProject<Projects.MyApi>("api");
builder.AddProject<Projects.MyWorker>("worker");

builder.Build().Run();
```

All services using `WithOtlpExporter()` will automatically send their telemetry to the Grafana stack.

## Custom ports

By default, the following ports are used:

| Port | Purpose |
|------|---------|
| 3000 | Grafana UI |
| 4317 | OTLP gRPC ingestion |
| 4318 | OTLP HTTP ingestion |

Override them if needed:

```csharp
builder.AddGrafana(
    grafanaPort: 3001,
    otlpGrpcPort: 4320,
    otlpHttpPort: 4321
);
```

## What's included in the stack

The `grafana/otel-lgtm` all-in-one container bundles:

- **OpenTelemetry Collector** — receives OTLP data from your services
- **Loki** — stores and queries logs
- **Tempo** — stores and queries distributed traces
- **Mimir** — stores and queries metrics
- **Grafana** — visualization and dashboards

## Notes

- The container runs with `ContainerLifetime.Persistent` so it survives AppHost restarts
- The Aspire dashboard's built-in Structured Logs, Traces, and Metrics tabs will no longer receive data since OTLP is redirected to Grafana
- The `grafana/otel-lgtm` image is intended for development and testing, not production

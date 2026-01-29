// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding Grafana LGTM (Loki, Grafana, Tempo, Mimir) observability stack
/// to a distributed application, replacing the built-in dashboard telemetry with Grafana.
/// </summary>
public static class GrafanaBuilderExtensions
{
    /// <summary>
    /// Adds a Grafana LGTM all-in-one container (<c>grafana/otel-lgtm</c>) to the application
    /// and redirects all OTLP telemetry from child services to Grafana instead of the Aspire dashboard.
    /// The Grafana UI is embedded as a tab in the Aspire dashboard.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="name">The name of the Grafana resource. Defaults to <c>"grafana"</c>.</param>
    /// <param name="grafanaPort">The host port for the Grafana UI. Defaults to 3000.</param>
    /// <param name="otlpGrpcPort">The host port for OTLP gRPC ingestion. Defaults to 4317.</param>
    /// <param name="otlpHttpPort">The host port for OTLP HTTP ingestion. Defaults to 4318.</param>
    /// <returns>The resource builder for the Grafana container.</returns>
    public static IResourceBuilder<ContainerResource> AddGrafana(
        this IDistributedApplicationBuilder builder,
        string name = "grafana",
        int grafanaPort = 3000,
        int otlpGrpcPort = 4317,
        int otlpHttpPort = 4318)
    {
        var grafana = builder.AddContainer(name, "grafana/otel-lgtm")
            .WithImageTag("latest")
            .WithHttpEndpoint(port: grafanaPort, targetPort: 3000, name: "grafana-ui")
            .WithEndpoint(port: otlpGrpcPort, targetPort: 4317, name: "otlp-grpc", scheme: "http")
            .WithEndpoint(port: otlpHttpPort, targetPort: 4318, name: "otlp-http", scheme: "http")
            .WithLifetime(ContainerLifetime.Persistent);

        // Override the OTLP endpoint so all child services send telemetry to Grafana's OTel Collector
        // instead of the Aspire dashboard's built-in OTLP receiver.
        builder.Configuration[KnownConfigNames.DashboardOtlpGrpcEndpointUrl] = $"http://localhost:{otlpGrpcPort}";
        builder.Configuration[KnownConfigNames.DashboardOtlpHttpEndpointUrl] = $"http://localhost:{otlpHttpPort}";

        // Pass the Grafana UI URL to the dashboard so it can embed it in an iframe tab.
        builder.Configuration[KnownConfigNames.DashboardGrafanaUrl] = $"http://localhost:{grafanaPort}";

        return grafana;
    }
}

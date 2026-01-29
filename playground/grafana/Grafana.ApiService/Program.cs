// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

var activitySource = new ActivitySource("Grafana.ApiService");
var meter = new Meter("Grafana.ApiService");
var requestCounter = meter.CreateCounter<long>("api.requests", description: "Total API requests");

app.MapDefaultEndpoints();

app.MapGet("/", () =>
{
    requestCounter.Add(1);
    app.Logger.LogInformation("Root endpoint hit");

    using var activity = activitySource.StartActivity("ProcessRootRequest");
    activity?.SetTag("endpoint", "/");

    return "Hello from Grafana sample!";
});

app.MapGet("/weather", () =>
{
    requestCounter.Add(1);

    using var activity = activitySource.StartActivity("GenerateWeatherForecast");

    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

    var forecast = Enumerable.Range(1, 5).Select(index =>
    {
        var temp = Random.Shared.Next(-20, 55);
        var summary = summaries[Random.Shared.Next(summaries.Length)];

        activity?.AddEvent(new ActivityEvent("ForecastGenerated", tags: new ActivityTagsCollection
        {
            { "day", index },
            { "temperatureC", temp },
            { "summary", summary }
        }));

        app.Logger.LogInformation("Day {Day}: {TempC}°C, {Summary}", index, temp, summary);

        return new { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)), TemperatureC = temp, Summary = summary };
    }).ToArray();

    return forecast;
});

app.Run();

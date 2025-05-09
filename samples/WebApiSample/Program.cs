using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetryExtensions.Extensions;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddTelemetry();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/hello", () => 
{
    var activitySource = new ActivitySource("WebApiSample");
    using var activity = activitySource.StartActivity("HelloEndpoint");
    activity?.SetTag("endpoint", "/hello");
    
    return "Hello from OpenTelemetry-enabled Web API!";
});

app.Run();

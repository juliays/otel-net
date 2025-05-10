using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTelemetryExtensions.Configuration;
using System.Text.Json;

namespace OpenTelemetryExtensions.Tests
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void TelemetryConfig_DeserializesCorrectly()
        {
            var json = @"{
                ""resource"": {
                    ""environment"": ""test"",
                    ""component"": ""test-api"",
                    ""workspaceId"": ""test-workspace-id"",
                    ""mnd-applicationid"": ""app-12345"",
                    ""cloud_provider"": ""azure"",
                    ""opt-dora"": ""false"",
                    ""opt-service-id"": ""srv-12345""
                },
                ""Serilog"": {
                    ""MinimumLevel"": {
                        ""Default"": ""Information"",
                        ""Override"": {
                            ""Microsoft"": ""Warning"",
                            ""System"": ""Warning""
                        }
                    },
                    ""WriteTo"": [
                        {
                            ""Name"": ""Console"",
                            ""Args"": {
                                ""formatter"": ""Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact""
                            }
                        }
                    ],
                    ""Enrich"": [""FromLogContext"", ""WithMachineName"", ""WithThreadId""]
                },
                ""exporters"": {
                    ""console"": {
                        ""enabled"": true,
                        ""includeScopes"": true
                    },
                    ""appInsights"": {
                        ""enabled"": true,
                        ""connectionString"": ""test-connection-string""
                    },
                    ""datadog"": {
                        ""enabled"": false,
                        ""endpoint"": ""https://api.datadoghq.com"",
                        ""apiKey"": ""test-api-key""
                    }
                },
                ""tracer"": {
                    ""sampleRate"": 0.5
                }
            }";
            
            var config = JsonSerializer.Deserialize<TelemetryConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.IsNotNull(config);
            Assert.AreEqual("test", config.Resource.Environment);
            Assert.AreEqual("test-api", config.Resource.Component);
            Assert.AreEqual("test-workspace-id", config.Resource.WorkspaceId);
            Assert.AreEqual("app-12345", config.Resource.ApplicationId);
            Assert.AreEqual("azure", config.Resource.CloudProvider);
            Assert.AreEqual("false", config.Resource.OptDora);
            Assert.AreEqual("srv-12345", config.Resource.OptServiceId);
            
            Assert.IsTrue(config.Exporters.Console.Enabled);
            Assert.IsTrue(config.Exporters.Console.IncludeScopes);
            Assert.IsTrue(config.Exporters.AppInsights.Enabled);
            Assert.AreEqual("test-connection-string", config.Exporters.AppInsights.ConnectionString);
            Assert.IsFalse(config.Exporters.Datadog.Enabled);
            Assert.AreEqual("https://api.datadoghq.com", config.Exporters.Datadog.Endpoint);
            Assert.AreEqual("test-api-key", config.Exporters.Datadog.ApiKey);
            
            Assert.AreEqual(0.5, config.Tracer.SampleRate);
        }
    }
}

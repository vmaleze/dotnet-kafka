using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace DotnetKafka
{
    public static class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] parameters)
        {
            return WebHost.CreateDefaultBuilder()
                .UseUrls("http://*:8080")
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.Limits.KeepAliveTimeout =
                        TimeSpan.FromMinutes(720);
                })
                .UseSerilog();
        }

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .AddLoggerConfiguration("kafka-dotnet")
                .CreateLogger();

            try
            {
                Log.Information("Initializing service");
                var webHost = CreateWebHostBuilder(args).Build();
                webHost.Run();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Service unexpectedly terminated");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        
        public static LoggerConfiguration AddLoggerConfiguration(
            this LoggerConfiguration loggerConfiguration,
            string serviceNamespace = "<unknown>",
            string level = "Information")
        {
            var levelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = !string.IsNullOrEmpty(level) && Enum.TryParse<LogEventLevel>(level, true, out var value) ? value : LogEventLevel.Information
            };

            var overrideLevel = levelSwitch.MinimumLevel == LogEventLevel.Information ? LogEventLevel.Warning : levelSwitch.MinimumLevel;

            var logger = loggerConfiguration
                .MinimumLevel.ControlledBy(levelSwitch)
                .MinimumLevel.Override("Microsoft", overrideLevel)
                .MinimumLevel.Override("System", overrideLevel)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: $"[{{Timestamp:yyyy-MM-dd HH:mm:ss.fff}} {serviceNamespace} {{Level:u3}}] {{Message:j}}{{NewLine}}{{Exception}}",
                    theme: AnsiConsoleTheme.Code);

            return logger;
        }
    }
}

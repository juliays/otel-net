using Microsoft.Extensions.Logging;

namespace Telemetry.Extensions;

/// <summary>
/// Provides LoggerMessage-based extension methods for high-performance logging with source generation.
/// This is to address the performance issues with the traditional logging approach.
/// The source generator creates static methods for logging at compile time.
/// This allows for better performance and avoids the overhead of reflection.
/// </summary>
public static partial class LoggerExtensions
{
    /// <summary>
    /// Logs an information message.
    /// </summary>
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "{Message}")]
    public static partial void Information(this ILogger logger, string message);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "{Message}")]
    public static partial void Error(this ILogger logger, string message);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "{Message}")]
    public static partial void Debug(this ILogger logger, string message);

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Trace,
        Message = "{Message}")]
    public static partial void Trace(this ILogger logger, string message);

    /// <summary>
    /// Logs an exception w/.
    /// </summary>
    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "An exception occurred: {Message}")]
    public static partial void Exception(this ILogger logger, string message, Exception exception);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "{Message}")]
    public static partial void Warning(this ILogger logger, string message);
}

namespace Telemetry.Exceptions;

/// <summary>
/// Exception for when Telemetry lib is unable to perform some kind of action.
/// </summary>
public class TelemetryException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryException"/> class.
    /// </summary>
    public TelemetryException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryException"/> class with a message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public TelemetryException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryException"/> class with a message and reference to an inner exception.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner Exception reference.</param>
    public TelemetryException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

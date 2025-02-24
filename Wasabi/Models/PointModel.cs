namespace Wasabi.Models;

public class Point
{
    /// <summary>
    ///     Gets the date and time when the point entry was created.
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    ///     Gets the severity level of the point entry.
    /// </summary>
    public int Severity { get; init; }

    /// <summary>
    ///     Gets the cause or reason for the point entry.
    /// </summary>
    public required string Cause { get; init; }
}
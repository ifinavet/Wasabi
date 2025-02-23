using System.Text.Json;

namespace Wasabi.Services;

/// <summary>
///     Represents a single point entry with date, severity, and cause information.
/// </summary>
public class PointEntry
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

/// <summary>
///     Defines the contract for services that handle point entry operations.
/// </summary>
public interface IPointsService
{
    /// <summary>
    ///     Parses a string containing point entries into an array of PointEntry objects.
    /// </summary>
    /// <param name="pointEntriesString">The string containing point entries to parse. Each line should be a valid JSON object.</param>
    /// <returns>An array of parsed PointEntry objects, or null if parsing fails or input is invalid.</returns>
    public List<PointEntry>? ParsedPointEntries(string? pointEntriesString);
}

/// <summary>
///     Provides functionality for parsing and managing point entries.
/// </summary>
public class PointsService : IPointsService
{
    /// <inheritdoc />
    public List<PointEntry>? ParsedPointEntries(string? pointEntriesString)
    {
        if (string.IsNullOrWhiteSpace(pointEntriesString))
            return null;

        try
        {
            string[] lines = pointEntriesString.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            List<PointEntry> entries = new(lines.Length);
            entries.AddRange(lines.Select(line => JsonSerializer.Deserialize<PointEntry>(line)).OfType<PointEntry>());

            return entries.Count > 0 ? entries.ToList() : null;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error!! \n" + e);
            return null;
        }
    }
}
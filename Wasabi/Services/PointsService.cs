using System.Text.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

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

    /// <summary>
    ///     Assigns points to a member based on the given cause and severity.
    /// </summary>
    /// <param name="member">The member to whom points are assigned.</param>
    /// <param name="cause">The reason for assigning points.</param>
    /// <param name="severity">The severity level of the points assigned.</param>
    public void GivePoints(IMember member, string cause, int severity);

    /// <summary>
    ///     Removes expired points from a member's point entries based on the given expiratory limit.
    /// </summary>
    /// <param name="member">The member whose expired points are to be removed.</param>
    /// <param name="expiratoryLimitInMonths">The time limit after which points are considered expired.</param>
    public void RemoveExpiredPoints(IMember member, int expiratoryLimitInMonths);
}

/// <summary>
///     Provides functionality for parsing and managing point entries.
/// </summary>
public class PointsService : IPointsService
{
    private readonly IMemberService _memberService;

    public PointsService(IMemberService memberService)
    {
        _memberService = memberService;
    }

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

    /// <inheritdoc />
    public void GivePoints(IMember member, string cause, int severity = 1)
    {
        DateTime violationDateTime = DateTime.Now;
        string violation = JsonSerializer.Serialize(new PointEntry
        {
            Date = violationDateTime,
            Severity = severity,
            Cause = cause
        });


        if (member.GetValue<string>("points") == null)
            member.SetValue("points", violation);
        else
            member.SetValue("points",
                member.GetValue<string>("points") + Environment.NewLine + violation);

        _memberService.Save(member);
    }

    public void RemoveExpiredPoints(IMember member, int expiratoryLimitInMonths)
    {
        List<PointEntry> pointEntries =
            ParsedPointEntries(member.GetValue<string>("points")) ?? throw new NullReferenceException();

        IEnumerable<PointEntry> expiredPoints = pointEntries.ToList()
            .Where(entry => entry.Date.AddMonths(expiratoryLimitInMonths) < DateTime.Now);
        foreach (PointEntry expiredPoint in expiredPoints) pointEntries.Remove(expiredPoint);

        string updatedPoints = string.Join(Environment.NewLine,
            pointEntries.Select(p => JsonSerializer.Serialize(p)));
        member.SetValue("points", updatedPoints);
        _memberService.Save(member);
    }
}
namespace HnWasm.Helpers;

internal static class TimeExtensions
{
    public static string UnixToHuman(this int unixSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).Elapsed().TimespanToHuman();

    public static string TimespanToHuman(this TimeSpan age)
    {
        const double daysInYear = 365.2425;
        const double daysInMonth = daysInYear / 12;
        if (age.TotalHours < 1)
        {
            return age.TotalMinutes.FloorToInt().AddUnit("minute");
        }
        var days = age.TotalDays;
        if (days < 1)
        {
            return age.TotalHours.FloorToInt().AddUnit("hour");
        }
        if (days < daysInMonth)
        {
            return days.FloorToInt().AddUnit("day");
        }
        if (days < daysInYear)
        {
            return (days / daysInMonth).FloorToInt().AddUnit("month");
        }
        return (days / daysInYear).FloorToInt().AddUnit("year");
    }

    static TimeSpan Elapsed(this DateTimeOffset then) => DateTimeOffset.UtcNow - then;

    static string AddUnit(this int input, string unit) => $"{input} {unit}{(input == 1 ? "" : "s")}";

    static int FloorToInt(this double input) => (int)Math.Floor(input);
}
using System.Globalization;

record Assignment(string Name, DateOnly Due, DateOnly Ready, long Estimate, long Done)
{
    public long Remaining => Estimate - Done;
}

sealed class StudyWeek(DateOnly start, long daily)
{
    public DateOnly Start { get; } = start;
    public List<Assignment> Tasks { get; } = [];
    private readonly Dictionary<DayOfWeek, long> weekdays = [];
    private readonly Dictionary<DateOnly, long> dates = [];

    public long Capacity(DateOnly day) => dates.GetValueOrDefault(day, weekdays.GetValueOrDefault(day.DayOfWeek, daily));

    public static DateOnly Date(string text) => DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static long Minutes(string text)
    {
        if (!long.TryParse(text, out long n) || n < 0) throw new FormatException("Minutes must be a nonnegative integer.");
        return n;
    }

    public static StudyWeek Read(TextReader reader)
    {
        StudyWeek? week = null;
        int lineNo = 0;
        while (reader.ReadLine() is { } line)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;
            var f = line.Split('\t', StringSplitOptions.TrimEntries);
            try
            {
                if (f.Any(string.IsNullOrEmpty)) throw new FormatException("Empty column.");
                if (week is null)
                {
                    if (f.Length != 2) throw new FormatException("Start with a date and daily minutes.");
                    week = new StudyWeek(Date(f[0]), Minutes(f[1]));
                }
                else if (f[0] == "day" && f.Length == 3)
                {
                    if (!week.dates.TryAdd(Date(f[1]), Minutes(f[2]))) throw new FormatException("Duplicate date override.");
                }
                else if (f[0] == "week" && f.Length == 3)
                {
                    if (!Enum.GetNames<DayOfWeek>().Contains(f[1], StringComparer.OrdinalIgnoreCase))
                        throw new FormatException("Use an English weekday name.");
                    var weekday = Enum.Parse<DayOfWeek>(f[1], true);
                    if (!week.weekdays.TryAdd(weekday, Minutes(f[2]))) throw new FormatException("Duplicate weekday override.");
                }
                else
                {
                    if (f.Length is < 3 or > 5) throw new FormatException("Use due date, task, estimate, optional available-from date and completed minutes.");
                    var due = Date(f[0]);
                    var ready = f.Length >= 4 ? Date(f[3]) : week.Start;
                    var estimate = Minutes(f[2]);
                    long done = f.Length == 5 ? Minutes(f[4]) : 0;
                    if (done > estimate) throw new FormatException("Completed minutes exceed the estimate.");
                    week.Tasks.Add(new(f[1], due, ready, estimate, done));
                }
            }
            catch (FormatException e) { throw new FormatException($"Line {lineNo}: {e.Message}", e); }
        }
        if (week is null || week.Tasks.Count == 0) throw new FormatException("Include a start date, a budget and at least one assignment.");
        return week;
    }
}

using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- <input-file|->");
    return args.Length == 1 ? 0 : 2;
}

try
{
    string[] lines = args[0] == "-" ? Console.In.ReadToEnd().ReplaceLineEndings("\n").TrimEnd('\n').Split('\n') : File.ReadAllLines(args[0]);
    Check(lines.Length >= 2 && lines.Any(line => !string.IsNullOrWhiteSpace(line)), "Input is empty or missing records.");
    var config = Fields(lines[0], 2);
    var start = Day(config[0]);
    var daily = Whole(config[1], 1);
    long required = 0;
    foreach (var task in lines.Skip(1).Select(s => Fields(s, 3)).OrderBy(t => Day(t[0])))
    {
        var due = Day(task[0]);
        required = checked(required + Whole(task[2], 0));
        var days = Math.Max(0L, due.DayNumber - start.DayNumber + 1L);
        var slack = checked(days * daily - required);
        Console.WriteLine($"{due:yyyy-MM-dd}  {task[1]}  {(slack >= 0 ? $"{slack} min spare" : $"short by {-slack} min")}");
    }

    return 0;
}
catch (Exception e)when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException or DivideByZeroException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

static void Check(bool valid, string message)
{
    if (!valid)
        throw new FormatException(message);
}

static string[] Fields(string line, int count)
{
    var fields = line.Split('\t', StringSplitOptions.TrimEntries);
    Check(fields.Length == count && fields.All(s => s.Length > 0), $"Expected {count} tab-separated values in '{line}'.");
    return fields;
}

static long Whole(string value, long minimum)
{
    var number = long.Parse(value, CultureInfo.InvariantCulture);
    Check(number >= minimum, $"Expected an integer of at least {minimum}.");
    return number;
}

static DateOnly Day(string value) => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

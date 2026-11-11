using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length is not (1 or 3) || args[0] == "--help" || (args.Length == 3 && args[1] != "--off"))
{
    Console.WriteLine("Usage: dotnet run -- <input-file|-> [--off YYYY-MM-DD]");
    return args is ["--help"] ? 0 : 2;
}

try
{
    using var reader = args[0] == "-" ? Console.In : File.OpenText(args[0]);
    var input = StudyWeek.Read(reader);
    var plan = Planner.Schedule(input);
    Console.WriteLine("Study plan");
    Planner.Print(input, plan);
    if (args.Length == 3)
    {
        var missed = StudyWeek.Date(args[2]);
        if (missed < input.Start) throw new FormatException("The day off precedes the planning start.");
        var alternative = Planner.Schedule(input, missed);
        Console.WriteLine($"\nIf {missed:yyyy-MM-dd} becomes unavailable");
        Planner.Compare(input, plan, alternative);
    }
    return 0;
}
catch (Exception e) when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

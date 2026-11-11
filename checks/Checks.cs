using System.Diagnostics;

// Run from the repository root after dotnet build.
static (int Code, string Output) Run(string input, params string[] options)
{
    var start = new ProcessStartInfo("dotnet")
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    start.ArgumentList.Add(Path.Combine("bin", "Debug", "net10.0", "deadline-load.dll"));
    start.ArgumentList.Add("-");
    foreach (var option in options)
        start.ArgumentList.Add(option);
    using var child = Process.Start(start)!;
    var stdout = child.StandardOutput.ReadToEndAsync();
    var stderr = child.StandardError.ReadToEndAsync();
    child.StandardInput.Write(input);
    child.StandardInput.Close();
    child.WaitForExit();
    return (child.ExitCode, (stdout.Result + stderr.Result).ReplaceLineEndings("\n").Trim());
}

void Check(string input, string[] expected, params string[] options)
{
    var(code, output) = Run(input, options);
    if (code != 0 || expected.Any(s => !output.Contains(s)))
        throw new Exception(output);
}

Check("2026-04-01\t60\n2026-04-02\tQuiz\t50\n2026-04-02\tEssay\t90", ["50 min  Quiz", "short by 20 min", "Essay: finish 2026-04-03"]);
Check("2026-04-01\t0\n2026-04-01\tEssay\t30", ["30 min left at planning horizon", "short by 30 min"]);
Check("2026-04-01\t30\n2026-04-03\tDraft\t30\t2026-04-03\t10", ["2026-04-03  20/30 min", "on time"]);
Check("2026-04-01\t30\n2026-04-01\tDone\t10\t2026-04-01\t10", ["already complete"]);
Check("2026-04-01\t60\nweek\tWednesday\t0\nday\t2026-04-01\t20\n2026-04-01\tQuiz\t20", ["2026-04-01  20/20 min", "Quiz: 20 -> 0 min", "short by 20 min at deadline (was 0)"], "--off", "2026-04-01");
Check("2026-04-01\t60\n2026-03-31\tOld\t60\n2026-04-01\tNew\t60", ["New: finish 2026-04-01, on time", "Old: finish 2026-04-02, 2 day(s) late"]);
Check("2026-04-01\t60\n2026-04-01\tBlocked\t20\t2026-04-03", ["Blocked: finish 2026-04-03", "short by 20 min"]);
Check("2026-04-01\t60\n2026-04-02\tShort\t10", ["assignment blocks are unchanged"], "--off", "2026-04-02");
Check(File.ReadAllText("semester.tsv"), ["Problem set: 10 -> 0 min", "Lab write-up: 45 -> 60 min", "short by 5 min at deadline"], "--off", "2026-11-11");
if (Run("2026-04-01\t10\n2026-04-01\tX\t5\t2026-04-01\t6").Code == 0)
    throw new Exception("Invalid progress accepted.");
Console.WriteLine("10 planning scenarios passed.");

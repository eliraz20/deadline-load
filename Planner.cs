record Block(int Day, int Task, long Minutes, int? StartMinute);
sealed record Plan(List<Block> Blocks, long[] Shortfall, long[] Remaining, int?[] Finish);

static class Planner
{
    public static Plan Schedule(StudyWeek input, DateOnly? off = null)
    {
        var tasks = input.Tasks;
        var remaining = tasks.Select(t => t.Remaining).ToArray();
        var missed = (long[])remaining.Clone();
        var finish = new int?[tasks.Count];
        var blocks = new List<Block>();
        var order = Enumerable.Range(0, tasks.Count).OrderBy(i => tasks[i].Due).ThenByDescending(i=>tasks[i].Continuous).ToArray();
        int start = input.Start.DayNumber;
        int cutoff = Math.Min(DateOnly.MaxValue.DayNumber, Math.Max(start, tasks.Max(t => t.Due.DayNumber)) + 28);
        for (int day = start; day <= cutoff; day++)
        {
            if (remaining.All(v => v == 0)) break;
            var date = DateOnly.FromDayNumber(day);
            if(off==date) continue;
            foreach(var session in input.Sessions(date))
            {
            long free = session.Minutes;

            // Protect work still on time, then spend capacity left that day on late assignments.
            foreach (bool late in new[] { false, true })
            {
                foreach (int i in order)
                {
                    if (free == 0) break;
                    var task = tasks[i];
                    if (remaining[i] == 0 || task.Ready > date || (task.Due < date) != late) continue;
                    if (task.Continuous && remaining[i]>free) continue;
                    long used = Math.Min(free, remaining[i]);
                    free -= used;
                    remaining[i] -= used;
                    if (!late) missed[i] -= used;
                    blocks.Add(new(day, i, used, session.Start.HasValue ? session.Start.Value + (int)(session.Minutes-free-used) : null));
                    if (remaining[i] == 0) finish[i] = day;
                }
            }
            }
        }
        return new(blocks, missed, remaining, finish);
    }

    private static string Status(Assignment task, Plan plan, int i)
    {
        if (task.Remaining == 0) return "already complete";
        if (plan.Finish[i] is not int day) return $"{plan.Remaining[i]} min left at planning horizon";
        var finish = DateOnly.FromDayNumber(day);
        return day > task.Due.DayNumber ? $"finish {finish:yyyy-MM-dd}, {day - task.Due.DayNumber} day(s) late" : $"finish {finish:yyyy-MM-dd}, on time";
    }

    public static void Print(StudyWeek input, Plan plan)
    {
        foreach (var group in plan.Blocks.GroupBy(b => b.Day))
        {
            var date = DateOnly.FromDayNumber(group.Key);
            var booked = group.Sum(b => b.Minutes);
            Console.WriteLine($"{date:yyyy-MM-dd}  {booked}/{input.Capacity(date)} min");
            foreach (var block in group)
            {
                string clock=block.StartMinute is int t ? $"{t/60:00}:{t%60:00}  " : "";
                Console.WriteLine($"  {clock}{block.Minutes} min  {input.Tasks[block.Task].Name}");
            }
        }
        Console.WriteLine("\nDeadlines");
        foreach (int i in Enumerable.Range(0, input.Tasks.Count).OrderBy(i => input.Tasks[i].Due))
        {
            var task = input.Tasks[i];
            Console.WriteLine($"{task.Due:yyyy-MM-dd}  {task.Name}: {Status(task, plan, i)}");
            if (plan.Shortfall[i] > 0)
                Console.WriteLine($"  short by {plan.Shortfall[i]} min at deadline");
        }
    }

    public static void Compare(StudyWeek input, Plan original, Plan revised)
    {
        bool changed = false;
        var before = original.Blocks.GroupBy(b => (b.Day, b.Task)).ToDictionary(g=>g.Key,g=>g.Sum(b=>b.Minutes));
        var after = revised.Blocks.GroupBy(b => (b.Day, b.Task)).ToDictionary(g=>g.Key,g=>g.Sum(b=>b.Minutes));
        foreach (var key in before.Keys.Union(after.Keys).OrderBy(k => k.Day).ThenBy(k => k.Task))
        {
            long old = before.GetValueOrDefault(key), now = after.GetValueOrDefault(key);
            if (old == now) continue;
            Console.WriteLine($"{DateOnly.FromDayNumber(key.Day):yyyy-MM-dd}  {input.Tasks[key.Task].Name}: {old} -> {now} min");
            changed = true;
        }
        for (int i = 0; i < input.Tasks.Count; i++)
        {
            if (original.Finish[i] == revised.Finish[i] && original.Shortfall[i] == revised.Shortfall[i] &&
                original.Remaining[i] == revised.Remaining[i]) continue;
            Console.WriteLine($"{input.Tasks[i].Name}: {Status(input.Tasks[i], revised, i)}; short by {revised.Shortfall[i]} min at deadline (was {original.Shortfall[i]})");
        }
        if (!changed) Console.WriteLine("The assignment blocks are unchanged.");
    }
}

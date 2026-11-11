# Deadline Load

Plan how to spend your available study minutes on dated assignments. See each day's work, which assignment will slip, the earliest finish in this schedule, and exactly what moves if you lose a study day.

```sh
dotnet run -- semester.tsv --off 2026-11-11
```

In this example Tuesday is booked, Wednesday has half the usual time, and the lab write-up becomes available Wednesday. The problem set already has 20 minutes done. Removing Wednesday moves work between days and changes which deadlines can be met. The example reports both schedules' differences.

## Your own workload

Requires the .NET 10 SDK. Run `dotnet run -- example.tsv` for the original two-assignment case or pass `-` to read from standard input. Separate columns with tabs. Blank lines and comment lines beginning with `#` are ignored.

| Row | Columns in order |
|---|---|
| First | Planning start date, default daily minutes |
| Assignment | Due date, name, total estimated minutes, optional first available date, optional minutes completed before the plan |
| Weekly availability | `week`, weekday name such as `Saturday`, available minutes |
| One-day exception | `day`, date, available minutes |

Use YYYY-MM-DD dates. A specific date overrides a weekday budget. A zero budget blocks the day. An assignment with no first available date can start on the planning start date. An optional `--off YYYY-MM-DD` reruns the plan with zero capacity on that date and prints the changes by day and assignment.

## How the schedule works

On each day the scheduler spends available minutes on released work in earliest-deadline order. Input order breaks deadline ties. Any spare minutes can go toward missed work, oldest deadline first. Assignments can be split across days. The result uses daily minute budgets and assumes estimates are accurate.

A deadline includes that whole day. The shortfall counts the minutes of that assignment still outstanding when its deadline passed. The planner continues placing late work for up to 28 days after the last deadline or planning start, whichever is later. Any work still unplaced is shown explicitly.

Run `dotnet run --project checks/Checks.csproj` from this folder to check ten scheduling scenarios.

## Plan around split shifts

Run `dotnet run -- windows.tsv`. A `window` row gives date, start and end (HH:mm). Disjoint windows replace the budget for that date. Add `continuous` after completed minutes for an assignment requiring one uninterrupted block. The sample places flashcards into two short windows and defers a 45-minute mock exam until a sufficiently long window. Untimed budgets act as one block each day. The schedule is a deadline-first heuristic, with continuous assignments first on a tie.

## When to use it

A student planning around classes or a shift needs to know which work can fit into the actual gaps.

Example result:

```text
Study plan
2026-11-09  35/50 min
  09:00  25 min  Flashcards
  15:00  10 min  Flashcards
2026-11-10  45/50 min
  10:00  45 min  Mock oral exam

Deadlines
2026-11-09  Mock oral exam: finish 2026-11-10, 1 day(s) late
  short by 45 min at deadline
2026-11-09  Flashcards: finish 2026-11-09, on time
```

Related course or field workflow: [Lehigh fixed commitment calendar](https://studentaffairs.lehigh.edu/content/fixed-commitment-calendar-0).

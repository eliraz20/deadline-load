# Deadline Load

Check whether upcoming assignments fit a daily study budget.

## Try it

Install the .NET 10 SDK, open this folder, and run:

```sh
dotnet run -- example.tsv
```

Look for `short by 20` in the output. To use your own data, copy the example and pass the new filename. Use `-` to read from standard input.

## Input

First line: start date and minutes available per day. Following rows: due date, assignment, estimated minutes. Days include the start and due date. Separate columns with actual tabs, omit a header row, and use a period for decimal fractions.

Built for general studies coursework. [ISCED-F field 001](https://uis.unesco.org/sites/default/files/documents/isced-fields-of-education-and-training-2013-en.pdf).

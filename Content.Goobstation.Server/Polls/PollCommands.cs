using System.Linq;
using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.Polls;

[AdminCommand(AdminFlags.Admin)]
public sealed class CreatePollCommand : LocalizedEntityCommands
{
    [Dependency] private readonly PollManager _pollManager = default!;

    public override string Command => "createpoll";
    public override string Description => "Creates a new poll that players can vote on.";
    public override string Help => "Usage: createpoll <title> <description> <days> [allowMultiple] <option1> <option2> [option3...]";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 5)
        {
            shell.WriteError("Not enough arguments. Need at least: title, description, days, allowMultiple (true/false), and 2 options.");
            return;
        }

        var title = args[0];
        var description = args[1];

        if (!int.TryParse(args[2], out var days) || days < 0)
        {
            shell.WriteError("Invalid days value. Must be a positive integer or 0 for no end time.");
            return;
        }

        if (!bool.TryParse(args[3], out var allowMultiple))
        {
            shell.WriteError("Invalid allowMultiple value. Must be 'true' or 'false'.");
            return;
        }

        var options = args.Skip(4).ToList();
        if (options.Count < 2)
        {
            shell.WriteError("Need at least 2 options for the poll.");
            return;
        }

        DateTime? endTime = days > 0 ? DateTime.UtcNow.AddDays(days) : null;
        var userId = shell.Player?.UserId;

        var poll = await _pollManager.CreatePoll(title, description, options, endTime, allowMultiple, userId);

        if (poll != null)
        {
            shell.WriteLine($"Poll created successfully! ID: {poll.PollId}");
            shell.WriteLine($"Title: {poll.Title}");
            shell.WriteLine($"Options: {string.Join(", ", poll.Options.Select(o => o.OptionText))}");
            shell.WriteLine($"Ends: {(endTime.HasValue ? endTime.Value.ToString("yyyy-MM-dd HH:mm") : "Never")}");
        }
        else
        {
            shell.WriteError("Failed to create poll.");
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHint("Poll title"),
            2 => CompletionResult.FromHint("Poll description"),
            3 => CompletionResult.FromHint("Duration in days (0 for no end)"),
            4 => CompletionResult.FromHintOptions(["true", "false"], "Allow multiple choices"),
            _ => CompletionResult.FromHint($"Option {args.Length - 3}")
        };
    }
}

[AdminCommand(AdminFlags.Admin)]
public sealed class ClosePollCommand : LocalizedEntityCommands
{
    [Dependency] private readonly PollManager _pollManager = default!;

    public override string Command => "closepoll";
    public override string Description => "Closes an active poll.";
    public override string Help => "Usage: closepoll <poll id>";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Wrong number of arguments. Expected: poll id");
            return;
        }

        if (!int.TryParse(args[0], out var pollId))
        {
            shell.WriteError("Invalid poll ID.");
            return;
        }

        await _pollManager.ClosePoll(pollId);
        shell.WriteLine($"Poll {pollId} has been closed.");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
#pragma warning disable RA0004
            var polls = Task.Run(_pollManager.GetActivePolls).Result;
#pragma warning restore RA0004
            var options = polls.Select(p => new CompletionOption(p.PollId.ToString(), p.Title));
            return CompletionResult.FromHintOptions(options, "Active poll ID");
        }

        return CompletionResult.Empty;
    }
}

[AnyCommand]
public sealed class ListPollsCommand : LocalizedEntityCommands
{
    [Dependency] private readonly PollManager _pollManager = default!;

    public override string Command => "listpolls";
    public override string Description => "Lists all active polls.";
    public override string Help => "Usage: listpolls";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var polls = await _pollManager.GetActivePolls();

        if (polls.Count == 0)
        {
            shell.WriteLine("No active polls.");
            return;
        }

        shell.WriteLine("Active Polls:");
        foreach (var poll in polls)
        {
            shell.WriteLine($"[{poll.PollId}] {poll.Title}");
            shell.WriteLine($"  Description: {poll.Description}");
            shell.WriteLine($"  Options: {string.Join(", ", poll.Options.Select(o => $"{o.OptionText} ({o.VoteCount} votes)"))}");
            shell.WriteLine($"  Ends: {(poll.EndTime.HasValue ? poll.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : "Never")}");
            shell.WriteLine($"  Multiple Choice: {poll.AllowMultipleChoices}");
            shell.WriteLine("");
        }
    }
}

[AnyCommand]
public sealed class PollInfoCommand : LocalizedEntityCommands
{
    [Dependency] private readonly PollManager _pollManager = default!;

    public override string Command => "pollinfo";
    public override string Description => "Shows detailed information about a specific poll.";
    public override string Help => "Usage: pollinfo <poll id>";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Wrong number of arguments. Expected: poll id");
            return;
        }

        if (!int.TryParse(args[0], out var pollId))
        {
            shell.WriteError("Invalid poll ID.");
            return;
        }

        var poll = await _pollManager.GetPoll(pollId);

        if (poll == null)
        {
            shell.WriteError($"Poll {pollId} not found.");
            return;
        }

        shell.WriteLine($"Poll #{poll.PollId}: {poll.Title}");
        shell.WriteLine($"Description: {poll.Description}");
        shell.WriteLine($"Created by: {poll.CreatedByName ?? "Server"}");
        shell.WriteLine($"Started: {poll.StartTime:yyyy-MM-dd HH:mm}");
        shell.WriteLine($"Ends: {(poll.EndTime.HasValue ? poll.EndTime.Value.ToString("yyyy-MM-dd HH:mm") : "Never")}");
        shell.WriteLine($"Status: {(poll.Active ? "Active" : "Closed")}");
        shell.WriteLine($"Multiple Choice: {poll.AllowMultipleChoices}");
        shell.WriteLine("\nOptions:");

        var totalVotes = poll.Options.Sum(o => o.VoteCount);
        foreach (var option in poll.Options.OrderBy(o => o.DisplayOrder))
        {
            var percentage = totalVotes > 0 ? (option.VoteCount * 100.0 / totalVotes) : 0;
            shell.WriteLine($"  {option.DisplayOrder + 1}. {option.OptionText}: {option.VoteCount} votes ({percentage:F1}%)");
        }

        shell.WriteLine($"\nTotal Votes: {totalVotes}");
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
#pragma warning disable RA0004
            var polls = Task.Run(_pollManager.GetActivePolls).Result;
#pragma warning restore RA0004
            var options = polls.Select(p => new CompletionOption(p.PollId.ToString(), p.Title));
            return CompletionResult.FromHintOptions(options, "Poll ID");
        }

        return CompletionResult.Empty;
    }
}

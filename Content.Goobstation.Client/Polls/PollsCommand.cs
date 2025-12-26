using Robust.Client.UserInterface;
using Robust.Shared.Console;
using Content.Shared.Administration;

namespace Content.Goobstation.Client.Polls;

[AnyCommand]
public sealed class PollsCommand : LocalizedCommands
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    public override string Command => "polls";
    public override string Description => "Opens the community polls window.";
    public override string Help => "Usage: polls";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _uiManager.GetUIController<PollUIController>().TogglePollWindow();
    }
}

using Content.Shared.Administration;
using Robust.Client.UserInterface;
using Robust.Shared.Console;

namespace Content.Goobstation.Client.Achievements;

[AnyCommand]
public sealed partial class AchievementsCommand : LocalizedCommands
{
    [Dependency] private IUserInterfaceManager _ui = default!;

    public override string Command => "achievements";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _ui.GetUIController<AchievementsUIController>().ToggleWindow();
    }
}

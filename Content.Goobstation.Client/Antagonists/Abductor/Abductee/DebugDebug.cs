using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Content.Goobstation.Client.Antagonists.Abductor.Abductee;

[AnyCommand]
public sealed class ShowAbducteePopupCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

    public override string Command => "showabducteepopup";
    public override string Description => "Shows the abductee return popup for testing";
    public override string Help => "showabducteepopup";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError("This command cannot be run from the server.");
            return;
        }

        // Get the system instance
        var system = _entitySystem.GetEntitySystem<AbducteeReturnPopupSystem>();

        // Call the public method
        system.OpenAbducteePopup();
        shell.WriteLine("Abductee popup shown!");
    }
}

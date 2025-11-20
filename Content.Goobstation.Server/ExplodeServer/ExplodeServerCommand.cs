using Robust.Shared.Console;
using Content.Server.Administration;
using Content.Shared.Administration;

namespace Content.Goobstation.Server.ExplodeServer;

/// <inheritdoc/>
[AdminCommand(AdminFlags.Admin)]
public sealed class ExplodeServerCommand : LocalizedCommands
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    private const string CommandName = "explodeserver";
    public override string Command => CommandName;
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    { 
        _entityManager.System<ExplodeServerSystem>().TriggerOverlay();
    }
}

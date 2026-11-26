using Content.Goobstation.Common.CCVar;

namespace Content.Server.NPC.Pathfinding;

public sealed partial class PathfindingSystem
{
    private bool _disabled;

    private void InitializeGoob()
    {
        Subs.CVar(_cfg, GoobCVars.DisablePathfinding, x => _disabled = x, true);
    }
}

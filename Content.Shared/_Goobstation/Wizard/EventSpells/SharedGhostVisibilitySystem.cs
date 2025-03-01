using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard.EventSpells;

public abstract class SharedGhostVisibilitySystem : EntitySystem
{
    protected static readonly EntProtoId GameRule = "GhostsVisible";

    public bool GhostsVisible()
    {
        var query = EntityQueryEnumerator<GhostsVisibleRuleComponent, ActiveGameRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out _, out _, out _, out _))
        {
            return true;
        }

        return false;
    }
}

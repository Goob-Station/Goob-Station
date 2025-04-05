using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.EventSpells;

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

    public virtual bool IsVisible(GhostComponent component)
    {
        return false;
    }
}

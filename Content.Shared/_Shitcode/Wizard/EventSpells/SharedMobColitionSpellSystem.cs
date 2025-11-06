using Content.Shared.GameTicking.Components;

namespace Content.Shared._Goobstation.Wizard.EventSpells;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedMobCollisionSpellSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }
    public bool MobCollisionEnabled()
    {
        var query = EntityQueryEnumerator<MobCollisionSpellRuleComponent>();
        while (query.MoveNext(out var _, out var _))
        {
            return true;
        }
        return false;
    }
}

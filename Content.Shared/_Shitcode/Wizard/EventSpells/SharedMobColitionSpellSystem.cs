using Content.Shared.GameTicking.Components;

namespace Content.Shared._Goobstation.Wizard.EventSpells;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedMobCollisionSpellSystem : EntitySystem
{
    public override void Initialize()
    {

    }
    public bool MobCollisionEnabled()
    {
        var query = EntityQueryEnumerator<MobCollisionSpellRuleComponent, ActiveGameRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out _, out _, out _, out _))
        {
            return true;
        }

        return false;
    }

}

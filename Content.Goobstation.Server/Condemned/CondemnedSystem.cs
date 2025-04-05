using Content.Goobstation.Shared.CheatDeath;
using Content.Shared.Mobs;
using Content.Shared.Traits.Assorted;

namespace Content.Goobstation.Server.Condemned;

public sealed partial class CondemnedSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CondemnedComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, CondemnedComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (TryComp<CheatDeathComponent>(uid, out var cheatDeath) && cheatDeath.ReviveAmount > 0)
            return;

        EnsureComp<UnrevivableComponent>(uid);
        // Todo - Teleport your ass to HELL when hell-smite is done.

    }
}

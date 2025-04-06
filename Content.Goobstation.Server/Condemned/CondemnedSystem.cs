using Content.Goobstation.Shared.CheatDeath;
using Content.Shared.Examine;
using Content.Shared.Mobs;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Condemned;

public sealed partial class CondemnedSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CondemnedComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<CondemnedComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, CondemnedComponent comp, ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient && !comp.IsCorporateOwned)
            args.PushMarkup(Loc.GetString("condemned-component-examined", ("target", uid)));
    }

    private void OnMobStateChanged(EntityUid uid, CondemnedComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead && !comp.IsCorporateOwned)
            return;

        if (TryComp<CheatDeathComponent>(uid, out var cheatDeath) && cheatDeath.ReviveAmount > 0)
            return;

        EnsureComp<UnrevivableComponent>(uid);
        // Todo - Teleport your ass to HELL when hell-smite is done.

    }
}

using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

public sealed class SpikerLashSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpikerLashComponent, SpikerLashEvent>(OnSpikerLash);
    }

    private void OnSpikerLash(Entity<SpikerLashComponent> ent, ref SpikerLashEvent args)
    {
        _popup.PopupPredicted(Loc.GetString("wraith-spiker-lash", ("user", ent.Owner), ("target", args.Target)), ent.Owner, ent.Owner, PopupType.MediumCaution);
        _audio.PlayPredicted(ent.Comp.LashSound, ent.Owner, args.Target);
        _stunSystem.TryKnockdown(args.Target, ent.Comp.KnockdownDuration, true);

        if (!TryComp<BloodstreamComponent>(args.Target, out var blood))
            return;

        _bloodstream.TryModifyBloodLevel((args.Target, blood), ent.Comp.BleedAmount);
        _bloodstream.TryModifyBleedAmount((args.Target, blood), blood.MaxBleedAmount);

        args.Handled = true;
    }
}

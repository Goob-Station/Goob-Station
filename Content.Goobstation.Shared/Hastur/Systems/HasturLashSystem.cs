using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Hastur.Systems;

public sealed class HasturLashSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HasturLashComponent, HasturLashEvent>(OnLash);
    }

    private void OnLash(Entity<HasturLashComponent> ent, ref HasturLashEvent args)
    {
        _popup.PopupPredicted(
            Loc.GetString("hastur-lash-target", ("user", ent.Owner), ("target", args.Target)),
            ent.Owner, args.Target, PopupType.MediumCaution);
        _audio.PlayPredicted(ent.Comp.LashSound, ent.Owner, ent.Owner);
        _stunSystem.TryKnockdown(args.Target, ent.Comp.KnockdownDuration, true);

        if (!TryComp<BloodstreamComponent>(args.Target, out var blood))
            return;

        _bloodstream.TryModifyBloodLevel((args.Target, blood), ent.Comp.BleedAmount);
        _bloodstream.TryModifyBleedAmount((args.Target, blood), blood.MaxBleedAmount);

        args.Handled = true;
    }
}

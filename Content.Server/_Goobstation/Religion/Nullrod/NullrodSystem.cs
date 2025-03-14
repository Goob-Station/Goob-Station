using Content.Shared.Bible.Components;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Content.Server.Bible;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Server._Goobstation.Religion.Nullrod;

public sealed partial class NullRodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NullrodComponent, MeleeHitEvent>(OnMeleeHitEvent);
        SubscribeLocalEvent<NullrodComponent, AttemptShootEvent>(OnShootAttempt);
    }

    private void OnMeleeHitEvent(EntityUid uid, NullrodComponent comp, MeleeHitEvent args)
    {
        if (HasComp<BibleUserComponent>(args.User))
            return;

        if (_damageableSystem.TryChangeDamage(args.User, comp.DamageOnUntrainedUse, false, origin: uid) == null)
            return;

        _popupSystem.PopupEntity(Loc.GetString(comp.UntrainedUseString), args.User, args.User, PopupType.MediumCaution);

        _audio.PlayPvs(comp.UntrainedUseSound, args.User);
        args.Handled = true;
    }

    private void OnShootAttempt(EntityUid uid, NullrodComponent comp, ref AttemptShootEvent args)
    {
        if (HasComp<BibleUserComponent>(args.User))
            return;

        if (_damageableSystem.TryChangeDamage(args.User, comp.DamageOnUntrainedUse, false, origin: uid) == null)
            return;

        _popupSystem.PopupEntity(Loc.GetString(comp.UntrainedUseString), args.User, args.User, PopupType.MediumCaution);
        _audio.PlayPvs(comp.UntrainedUseSound, args.User);

        args.Cancelled = true; // This isn't canceling the shot, probably something to do with duel wielding
        // IMO it's perfectly fine to just kill you for shooting but whatev.
    }
}


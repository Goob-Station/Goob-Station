using Content.Shared.Bible.Components;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Goobstation.Religion.Nullrod;

public sealed partial class NullRodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NullrodComponent, MeleeHitEvent>(OnMeleeHitEvent);
    }

    private void OnMeleeHitEvent(EntityUid uid, NullrodComponent comp, MeleeHitEvent args)

    {

        if (HasComp<BibleUserComponent>(args.User))
            return;

        if (_damageableSystem.TryChangeDamage(args.User, comp.DamageOnUntrainedUse, false, origin: uid) != null)
        // If you set ignore resistances to true, it'll FUCK you up.
        {
            _popupSystem.PopupEntity(Loc.GetString(comp.UntrainedUseString), args.User, args.User, PopupType.MediumCaution);

            _audio.PlayPvs("/Audio/Effects/hallelujah.ogg", args.User); // Probably change this sound effect LOL
            args.Handled = true;

        }

    }
}


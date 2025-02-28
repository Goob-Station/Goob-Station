using Content.Server.Bible.Components;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Goobstation.Religion.Nullrod;

public sealed partial class NullRodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NullrodComponent, MeleeHitEvent>(OnMeleeHitEvent);
    }

    private void OnMeleeHitEvent(EntityUid uid, NullrodComponent comp, MeleeHitEvent args)

    {

        if (HasComp<BibleUserComponent>(args.User))
            return;

        if (_damageableSystem.TryChangeDamage(args.User, comp.SelfDamage, true, origin: uid) != null)

        {
            var selfFailMessage = comp.FailPopup;
            _popupSystem.PopupEntity(selfFailMessage, args.User, args.User, PopupType.MediumCaution);

            _audio.PlayPvs("/Audio/Effects/hallelujah.ogg", args.User); // Probably change this sound effect LOL
            args.Handled = true;

        }

    }
}


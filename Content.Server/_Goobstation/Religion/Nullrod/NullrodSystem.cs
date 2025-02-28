 using Content.Server.Bible.Components;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Goobstation.Religion.Nullrod;

public sealed partial class NullRodSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NullrodComponent, MeleeHitEvent>(OnMeleeHitEvent);
    }

    private void OnMeleeHitEvent(EntityUid uid, NullrodComponent comp, MeleeHitEvent args)

    {

        if (HasComp<BibleUserComponent>(args.User)) return;

        if (_damageableSystem.TryChangeDamage(args.User, comp.SelfDamage, false, origin: uid) != null)

        {

            _audio.PlayPvs("/Audio/Effects/hit_kick.ogg", args.User);

        }

    }
}


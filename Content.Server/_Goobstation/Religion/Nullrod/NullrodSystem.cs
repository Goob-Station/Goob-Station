/* using Content.Server.Bible.Components;
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
        SubscribeLocalEvent<NullrodComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid performer, Entity<NullrodComponent> ent, ref MeleeHitEvent args)
    {
        if (!TryComp<BibleUserComponent>(performer, out var BibleUserComp))
        {
            _audio.PlayPvs("/Audio/Effects/hit_kick.ogg", args.User);
            _damageableSystem.TryChangeDamage(args.User);
            return;
        }
    }
}

*/

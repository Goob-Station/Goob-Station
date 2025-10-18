using Content.Goobstation.Shared.Wrestler.Components;
using Content.Goobstation.Shared.Wrestler.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._White.Grab;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Wrestler.Systems;

public sealed class WrestleSlamSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly GrabThrownSystem _grab = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WrestlerSlamComponent, WrestlerSlamEvent>(OnSlam);
    }

    private void OnSlam(Entity<WrestlerSlamComponent> ent, ref WrestlerSlamEvent args)
    {
        var (uid, comp) = ent;
        var target = args.Target;

        if (args.Handled)
            return;

        if (!HasComp<WrestlerComponent>(uid))
        {
            _popup.PopupPredicted(Loc.GetString("not-wrestler"), uid, uid, PopupType.MediumCaution);
            return;
        }

        // Stun the target first
        _stun.TryStun(target, comp.StunDuration, false);
        _stun.TryKnockdown(target, comp.KnockdownDuration, true);

        _damage.TryChangeDamage(uid, comp.Damage, targetPart: TargetBodyPart.All);
        _popup.PopupPredicted(Loc.GetString("wrestler-slam-1"), uid, uid, PopupType.MediumCaution);

        _audio.PlayPredicted(comp.Sound, uid, uid);

        args.Handled = true;
    }
}

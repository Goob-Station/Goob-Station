using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodsuckerMesmerizeSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerMesmerizeEvent>(OnMesmerize);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerMesmerizeDoAfterEvent>(OnMesmerizeDoAfter);
    }

    private void OnMesmerize(Entity<BloodsuckerComponent> ent, ref BloodsuckerMesmerizeEvent args)
    {
        if (!TryComp(ent, out BloodsuckerMesmerizeComponent? comp))
            return;

        if (args.Target == EntityUid.Invalid || args.Target == ent.Owner)
            return;

        if (!TryUseCosts(ent, comp))
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            comp.StartDelay,
            new BloodsuckerMesmerizeDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize"), ent.Owner, ent.Owner, PopupType.Small);

        if (HasComp<BibleUserComponent>(args.Target))
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize-chaplain-fail"), args.Target, args.Target, PopupType.MediumCaution);
        }
        //"You feel your eyes burn for a while, but it passes." (Chaplain warning popup)
    }

    private void OnMesmerizeDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerMesmerizeDoAfterEvent args)
    {
        if (args.Target is not EntityUid target)
            return;

        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (!TryComp(ent, out BloodsuckerMesmerizeComponent? comp))
            return;

        var duration = TimeSpan.FromSeconds(comp.ParalyzeDuration);

        // Paralyze
        _stun.TryAddStunDuration(target, duration);
        _stun.TryKnockdown(target, duration, true);
        _status.TryAddStatusEffect(target, "Muted", duration, true);
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerMesmerizeComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity), -comp.HumanityCost);

        return true;
    }
}

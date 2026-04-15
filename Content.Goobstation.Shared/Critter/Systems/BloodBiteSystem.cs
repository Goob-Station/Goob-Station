using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Critter.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodBiteSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodBiteComponent, BloodBiteEvent>(OnStart);
        SubscribeLocalEvent<BloodBiteComponent, BloodBiteDoAfterEvent>(OnDoAfter);
    }

    private void OnStart(EntityUid uid, BloodBiteComponent comp, BloodBiteEvent args)
    {
        var target = args.Target;

        if (target == EntityUid.Invalid || target == uid)
            return;

        if (!InRange(uid, target, comp.Range))
            return;

        var feeding = EnsureComp<BloodFeedingComponent>(uid);
        feeding.Target = target;
        feeding.HasBitten = false;

        StartDoAfter(uid, target, comp.StartDelay, comp);
    }

    private void OnDoAfter(EntityUid uid, BloodBiteComponent comp, ref BloodBiteDoAfterEvent args)
    {
        if (args.Cancelled)
        {
            Stop(uid);
            return;
        }

        if (!TryComp(uid, out BloodFeedingComponent? feeding))
            return;

        var target = feeding.Target;

        if (target == EntityUid.Invalid || !Exists(target))
        {
            Stop(uid);
            return;
        }

        if (!InRange(uid, target, comp.Range))
        {
            Stop(uid);
            return;
        }

        if (!feeding.HasBitten)
        {
            feeding.HasBitten = true;

            var bleedAmount = comp.BloodDrainAmount * comp.InitialBleedMultiplier;
            TryBleed(target, bleedAmount);

            StartDoAfter(uid, target, comp.SipDelay, comp);
            return;
        }

        // Loop
        if (TryDrain(target, uid, comp))
        {
            _damageable.TryChangeDamage(uid, comp.HealSpecifier, true);

            if (comp.DrinkSound != null)
                _audio.PlayPredicted(comp.DrinkSound, uid, uid);
        }

        StartDoAfter(uid, target, comp.SipDelay, comp);
    }

    private void Stop(EntityUid uid)
    {
        RemCompDeferred<BloodFeedingComponent>(uid);
    }

    private void StartDoAfter(EntityUid uid, EntityUid target, float delay, BloodBiteComponent comp)
    {
        var args = new DoAfterArgs(
            EntityManager,
            uid,
            delay,
            new BloodBiteDoAfterEvent(),
            uid,
            target)
        {
            BreakOnMove = comp.BreakOnMove,
            BreakOnDamage = comp.BreakOnDamage,
            Hidden = comp.HiddenDoAfter
        };

        _doAfter.TryStartDoAfter(args);
    }

    private bool TryDrain(EntityUid target, EntityUid user, BloodBiteComponent comp)
    {
        if (!TryComp(target, out BloodstreamComponent? targetBlood))
            return false;

        var amount = FixedPoint2.New(comp.BloodDrainAmount);

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(target, targetBlood),
            -amount);

        if (comp.TransferBlood && TryComp(user, out BloodstreamComponent? userBlood))
        {
            _bloodstream.TryModifyBloodLevel(
                new Entity<BloodstreamComponent?>(user, userBlood),
                amount);
        }

        return true;
    }

    private void TryBleed(EntityUid target, float amount)
    {
        if (!TryComp(target, out BloodstreamComponent? blood))
            return;

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(target, blood),
            FixedPoint2.New(-amount));
    }

    private bool InRange(EntityUid a, EntityUid b, float range)
    {
        if (!TryComp(a, out TransformComponent? ta) ||
            !TryComp(b, out TransformComponent? tb))
            return false;

        if (ta.MapID != tb.MapID)
            return false;

        var delta = ta.WorldPosition - tb.WorldPosition;
        return MathF.Abs(delta.X) <= range && MathF.Abs(delta.Y) <= range;
    }
}

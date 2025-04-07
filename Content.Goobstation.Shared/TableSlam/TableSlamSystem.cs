using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions.Events;
using Content.Shared.Climbing.Components;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.TableSlam;

/// <summary>
/// This handles...
/// </summary>
public sealed class TableSlamSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ContestsSystem _contestsSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PullerComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<PullableComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<PostTabledComponent, DisarmAttemptEvent>(OnDisarmAttemptEvent);
    }

    private void OnDisarmAttemptEvent(Entity<PostTabledComponent> ent, ref DisarmAttemptEvent args)
    {
        if(!_random.Prob(ent.Comp.ParalyzeChance))
            return;

        _stunSystem.TryParalyze(ent, TimeSpan.FromSeconds(3), false);
        RemComp<PostTabledComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var tabledQuery = EntityQueryEnumerator<PostTabledComponent>();
        while (tabledQuery.MoveNext(out var uid, out var comp))
        {
            if (_gameTiming.CurTime >= comp.PostTabledShovableTime)
             RemComp<PostTabledComponent>(uid);
        }
    }

    private void OnMeleeHit(Entity<PullerComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.GrabStage < GrabStage.Suffocate
            || ent.Comp.Pulling == null)
            return;

        if(!TryComp<PullableComponent>(ent.Comp.Pulling, out var pullableComponent))
            return;

        if (args.Direction != null)
            return;
        if (args.HitEntities.Count is > 1 or 0)
            return;

        var target = args.HitEntities.ElementAt(0);
        if (!HasComp<BonkableComponent>(target)) // checks if its a table.
            return;

        var massContest = _contestsSystem.MassContest(ent, ent.Comp.Pulling.Value);
        var attemptChance = Math.Clamp(1 * massContest, 0, 1);
        var attemptRoundedToNearestQuarter = Math.Round(attemptChance * 4, MidpointRounding.ToEven) / 4;
        if(_random.Prob((float) attemptRoundedToNearestQuarter)) // base chance to table slam someone is 1 if your mass ratio is less than 1 then your going to have a harder time slamming somebody.
            TryTableSlam((ent.Comp.Pulling.Value, pullableComponent), ent, target);
    }

    public void TryTableSlam(Entity<PullableComponent> ent, Entity<PullerComponent> pullerEnt, EntityUid tableUid)
    {
        if(!_transformSystem.InRange(ent.Owner.ToCoordinates(), tableUid.ToCoordinates(), 2f ))
            return;

        _standing.Down(ent);

        _pullingSystem.TryStopPull(ent, ent.Comp, pullerEnt, ignoreGrab: true);
        _throwingSystem.TryThrow(ent, tableUid.ToCoordinates() , ent.Comp.BasedTabledForceSpeed, animated: false, doSpin: false);
        pullerEnt.Comp.NextStageChange = _gameTiming.CurTime.Add(TimeSpan.FromSeconds(3)); // prevent table slamming spam
        ent.Comp.BeingTabled = true;
    }

    private void OnStartCollide(Entity<PullableComponent> ent, ref StartCollideEvent args)
    {
        if(!ent.Comp.BeingTabled)
            return;

        if (!HasComp<BonkableComponent>(args.OtherEntity))
            return;

        var modifierOnGlassBreak = 1;
        if (TryComp<GlassTableComponent>(args.OtherEntity, out var glassTableComponent))
        {
            _damageableSystem.TryChangeDamage(args.OtherEntity, glassTableComponent.TableDamage, origin: ent, targetPart: TargetBodyPart.Torso);
            _damageableSystem.TryChangeDamage(args.OtherEntity, glassTableComponent.ClimberDamage, origin: ent);
            modifierOnGlassBreak = 2;
        }
        else
        {
            _damageableSystem.TryChangeDamage(ent,
                new DamageSpecifier()
                {
                    DamageDict = new Dictionary<string, FixedPoint2> { { "Blunt", ent.Comp.TabledDamage } },
                },
                targetPart: TargetBodyPart.Torso);
            _damageableSystem.TryChangeDamage(ent,
                new DamageSpecifier()
                {
                    DamageDict = new Dictionary<string, FixedPoint2> { { "Blunt", ent.Comp.TabledDamage } },
                });
        }

        _staminaSystem.TakeStaminaDamage(ent, ent.Comp.TabledStaminaDamage);
        _stunSystem.TryKnockdown(ent, TimeSpan.FromSeconds(3 * modifierOnGlassBreak), false);
        var postTabledComponent = EnsureComp<PostTabledComponent>(ent);
        postTabledComponent.PostTabledShovableTime = _gameTiming.CurTime.Add(TimeSpan.FromSeconds(3));
        ent.Comp.BeingTabled = false;

        //_audioSystem.PlayPvs("/Audio/Effects/thudswoosh.ogg", uid);
    }
}

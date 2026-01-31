using System.Linq;
using Content.Goobstation.Common.BlockTeleport;
using Content.Goobstation.Common.Physics;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems.Abilities;
using Content.Shared.Bed.Sleep;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Magic;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class SharedStarTouchSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMagicSystem _magic = default!;
    [Dependency] private readonly SharedStarMarkSystem _starMark = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedStarGazerSystem _starGazer = default!;
    [Dependency] private readonly SharedHereticAbilitySystem _hereticAbility = default!;
    [Dependency] private readonly SharedHereticSystem _heretic = default!;

    public static readonly EntProtoId StarTouchStatusEffect = "StatusEffectStarTouched";
    public static readonly EntProtoId DrowsinessStatusEffect = "StatusEffectDrowsiness";
    public const string StarTouchBeamDataId = "startouch";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StarTouchComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<StarTouchComponent, UseInHandEvent>(OnUseInHand);

        SubscribeLocalEvent<StarTouchedStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<StarTouchedStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);
    }

    private void OnUseInHand(Entity<StarTouchComponent> ent, ref UseInHandEvent args)
    {
        var starGazer = _starGazer.ResolveStarGazer(args.User, out var spawned);
        if (starGazer == null)
            return;

        args.Handled = true;

        _hereticAbility.InvokeTouchSpell(ent, args.User);

        if (spawned)
            return;

        _pulling.StopAllPulls(args.User);
        _transform.SetMapCoordinates(args.User, _transform.GetMapCoordinates(starGazer.Value.Owner));
    }

    private void OnRemove(Entity<StarTouchedStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        var target = args.Target;

        if (TerminatingOrDeleted(target))
            return;

        RemCompDeferred<BlockTeleportComponent>(target);
        RemCompDeferred<StarTouchedComponent>(target);
        RemCompDeferred<CosmicTrailComponent>(target);

        if (!TryComp(target, out ComplexJointVisualsComponent? joint))
            return;

        EntityUid? heretic = null;
        List<NetEntity> toRemove = new();
        foreach (var (netEnt, data) in joint.Data)
        {
            if (data.Id != StarTouchBeamDataId)
                continue;

            toRemove.Add(netEnt);

            if (!TryGetEntity(netEnt, out var entity) || TerminatingOrDeleted(entity))
                continue;

            heretic = entity;
        }

        if (toRemove.Count == joint.Data.Count)
            RemCompDeferred(target, joint);
        else if (toRemove.Count != 0)
        {
            foreach (var netEnt in toRemove)
            {
                joint.Data.Remove(netEnt);
            }

            Dirty(target, joint);
        }

        if (heretic == null || !TryComp(ent, out StatusEffectComponent? status) || status.EndEffectTime == null ||
            status.EndEffectTime > _timing.CurTime)
            return;

        _pulling.StopAllPulls(target);

        var targetXform = Transform(target);
        var newCoords = Transform(heretic.Value).Coordinates;
        PredictedSpawnAtPosition(ent.Comp.CosmicCloud, targetXform.Coordinates);
        _transform.SetCoordinates((target, targetXform, MetaData(target)), newCoords);
        PredictedSpawnAtPosition(ent.Comp.CosmicCloud, newCoords);

        // Applying status effects next tick, otherwise status effects system shits itself
        Timer.Spawn(0,
            () =>
            {
                _status.TryUpdateStatusEffectDuration(target,
                    SleepingSystem.StatusEffectForcedSleeping,
                    ent.Comp.SleepTime);
                _starMark.TryApplyStarMark(target);
            });
    }

    private void OnApply(Entity<StarTouchedStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        EnsureComp<StarTouchedComponent>(args.Target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<StarTouchedComponent>();
        while (query.MoveNext(out var uid, out var touch))
        {
            touch.Accumulator += frameTime;

            if (touch.Accumulator < touch.TickInterval)
                continue;

            touch.Accumulator = 0f;

            UpdateBeams((uid, touch));
        }
    }

    private void UpdateBeams(Entity<StarTouchedComponent, ComplexJointVisualsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp2, false))
            return;

        var hasStarBeams = false;

        foreach (var (netEnt, _) in ent.Comp2.Data.Where(x => x.Value.Id == StarTouchBeamDataId).ToList())
        {
            if (!TryGetEntity(netEnt, out var target) || TerminatingOrDeleted(target) ||
                !_transform.InRange(target.Value, ent.Owner, ent.Comp1.Range))
            {
                ent.Comp2.Data.Remove(netEnt);
                continue;
            }

            hasStarBeams = true;
        }

        Dirty(ent.Owner, ent.Comp2);

        if (hasStarBeams)
            return;

        _status.TryRemoveStatusEffect(ent, StarTouchStatusEffect);
    }

    private void OnAfterInteract(Entity<StarTouchComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (args.Target == null || args.Target == args.User)
            return;

        var (uid, comp) = ent;

        var target = args.Target.Value;

        if (!TryComp(target, out MobStateComponent? mobState))
            return;

        args.Handled = true;

        if (!_heretic.TryGetHereticComponent(args.User, out var hereticComp, out _) ||
            _heretic.TryGetHereticComponent(args.Target.Value, out var th, out _) && th.CurrentPath == "Cosmos")
        {
            PredictedQueueDel(uid);
            return;
        }

        if (_magic.IsTouchSpellDenied(target))
        {
            _hereticAbility.InvokeTouchSpell(ent, args.User);
            return;
        }

        var range = hereticComp.Ascended ? 2 : 1;
        var xform = Transform(args.User);
        _starMark.SpawnCosmicFieldLine(xform.Coordinates,
            Angle.FromDegrees(90f).RotateDir(xform.LocalRotation.GetDir()).AsFlag(),
            -range,
            range,
            0,
            hereticComp.PathStage);

        if (!HasComp<StarMarkComponent>(target))
        {
            _starMark.TryApplyStarMark((target, mobState));
            _hereticAbility.InvokeTouchSpell(ent, args.User);
            return;
        }

        _status.TryRemoveStatusEffect(target, SharedStarMarkSystem.StarMarkStatusEffect);
        _status.TryUpdateStatusEffectDuration(target, DrowsinessStatusEffect, comp.DrowsinessTime);

        if (_status.TryUpdateStatusEffectDuration(target, StarTouchStatusEffect, comp.Duration))
        {
            EnsureComp<BlockTeleportComponent>(target);
            var beam = EnsureComp<ComplexJointVisualsComponent>(target);
            beam.Data[GetNetEntity(args.User)] = new ComplexJointVisualsData(StarTouchBeamDataId, comp.BeamSprite);
            Dirty(target, beam);
            var trail = EnsureComp<CosmicTrailComponent>(target);
            trail.CosmicFieldLifetime = comp.CosmicFieldLifetime;
            trail.Strength = hereticComp.PathStage;
        }

        _hereticAbility.InvokeTouchSpell(ent, args.User);
    }
}

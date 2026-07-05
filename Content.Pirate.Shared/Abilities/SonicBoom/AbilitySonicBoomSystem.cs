using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Movement.Systems;
using Content.Shared.Random.Helpers;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Pirate.Shared.Abilities.SonicBoom;

/// <summary>
/// Handles the sonic boom ability for entities with <see cref="AbilitySonicBoomComponent"/>.
/// </summary>
public sealed partial class AbilitySonicBoomSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly MovementModStatusSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbilitySonicBoomComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AbilitySonicBoomComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<AbilitySonicBoomComponent, AbilitySonicBoomEvent>(OnBoom);
    }

    private void OnMapInit(Entity<AbilitySonicBoomComponent> entity, ref MapInitEvent args)
    {
        _actions.AddAction(entity.Owner, ref entity.Comp.Action, entity.Comp.ActionProto, entity.Owner);
    }

    private void OnShutdown(Entity<AbilitySonicBoomComponent> entity, ref ComponentShutdown args)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.Action);
    }

    private void OnBoom(Entity<AbilitySonicBoomComponent> entity, ref AbilitySonicBoomEvent args)
    {
        if (args.Handled || !_timing.IsFirstTimePredicted)
            return;

        var entityCoords = _transform.GetMoverCoordinates(entity);

        foreach (var target in _lookup.GetEntitiesInRange(entity, entity.Comp.FlingRadius, LookupFlags.Uncontained))
        {
            var random = PredictedRandom(entity.Owner, target);
            var thrownVec = NextThrowDir(random) + (_transform.GetMoverCoordinates(target).Position - entityCoords.Position);
            var lenSq = thrownVec.LengthSquared();

            if (lenSq < 0.000001f)
                continue;

            _throwing.TryThrow(
                target,
                Vector2.Normalize(thrownVec) * (entity.Comp.FlingStrength / (1.0f + lenSq)),
                pushbackRatio: 0.0f);
        }

        SpawnAttachedTo(entity.Comp.ShockwaveProto, Transform(entity).Coordinates);
        _audio.PlayPredicted(entity.Comp.Sound, entity.Owner, entity.Owner);

        _movement.TryAddMovementSpeedModDuration(
            entity,
            MovementModStatusSystem.FlashSlowdown,
            entity.Comp.SlowdownDuration,
            entity.Comp.Slowdown);

        _adminLogger.Add(LogType.Action, LogImpact.Medium, $"{ToPrettyString(entity):user} used the sonic boom ability.");

        args.Handled = true;
    }

    private System.Random PredictedRandom(EntityUid source, EntityUid target)
    {
        var sourceNet = GetNetEntity(source);
        var targetNet = GetNetEntity(target);
        var seed = SharedRandomExtensions.HashCodeCombine(
            (int) _timing.CurTick.Value,
            sourceNet.Id,
            targetNet.Id,
            0x50495241);

        return new System.Random(seed);
    }

    private static Vector2 NextThrowDir(System.Random random)
    {
        return random.NextAngle().RotateVec(new Vector2(random.NextFloat(0, 0.05f), 0));
    }
}

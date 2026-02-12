using System.Linq;
using Content.Goobstation.Common.Religion;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Explosion.Components;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedStarMarkSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedStaminaSystem _stam = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedBroadphaseSystem _broadphase = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public static readonly EntProtoId StarMarkStatusEffect = "StatusEffectStarMark";
    public static readonly EntProtoId CosmicField = "WallFieldCosmic";

    private const float CosmosPassiveStaminaHealInterval = 1f;
    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicFieldComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<CosmicFieldComponent, StartCollideEvent>(OnStartCollide);

        SubscribeLocalEvent<StarMarkStatusEffectComponent, StatusEffectAppliedEvent>(OnApply);
        SubscribeLocalEvent<StarMarkStatusEffectComponent, StatusEffectRemovedEvent>(OnRemove);

        SubscribeLocalEvent<StarMarkComponent, PullStoppedMessage>(OnPullStop);
        SubscribeLocalEvent<StarMarkComponent, PullStartedMessage>(OnPullStart);
    }

    private void OnPullStart(Entity<StarMarkComponent> ent, ref PullStartedMessage args)
    {
        if (args.PulledUid == ent.Owner)
            RegenerateContacts(ent.Owner);
    }

    private void OnPullStop(Entity<StarMarkComponent> ent, ref PullStoppedMessage args)
    {
        if (args.PulledUid == ent.Owner)
            RegenerateContacts(ent.Owner);
    }

    private void OnRemove(Entity<StarMarkStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (TerminatingOrDeleted(args.Target) || !TryComp(args.Target, out StarMarkComponent? mark))
            return;

        RemCompDeferred(args.Target, mark);
        RegenerateContacts(args.Target);
    }

    private void OnApply(Entity<StarMarkStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        EnsureComp<StarMarkComponent>(args.Target);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<CosmicTrailComponent, PhysicsComponent, TransformComponent>();
        while (query.MoveNext(out var trail, out var physics, out var xform))
        {
            if (trail.NextCosmicFieldTime > _timing.CurTime)
                continue;

            if (physics.LinearVelocity.LengthSquared() < 0.25)
                continue;

            trail.NextCosmicFieldTime = _timing.CurTime + trail.CosmicFieldPeriod;
            SpawnCosmicField(xform.Coordinates, trail.Strength, trail.CosmicFieldLifetime);
        }

        _accumulator += frameTime;

        if (_accumulator < CosmosPassiveStaminaHealInterval)
            return;

        _accumulator = 0f;

        var cosmicFieldQuery = GetEntityQuery<CosmicFieldComponent>();

        var query2 = EntityQueryEnumerator<CosmosPassiveComponent, SpeedModifiedByContactComponent, StaminaComponent, PhysicsComponent>();
        while (query2.MoveNext(out var uid, out var passive, out _, out var stam, out var phys))
        {
            if (!_physics.GetContactingEntities(uid, phys).Any(cosmicFieldQuery.HasComp))
                continue;

            _stam.TryTakeStamina(uid, passive.StaminaHeal, stam);
        }
    }

    private void OnStartCollide(Entity<CosmicFieldComponent> ent, ref StartCollideEvent args)
    {
        if (args.OurFixture.Hard || ent.Comp.Strength < 7)
            return;

        var other = args.OtherEntity;

        if (!TryComp(other, out ActiveTimerTriggerComponent? trigger))
            return;

        // Defuse bombs
        RemComp(other, trigger);

        if (_net.IsClient)
            return;

        _audio.PlayPvs(ent.Comp.BombDefuseSound, other);
        _popup.PopupEntity(Loc.GetString(ent.Comp.BombDefusePopup, ("bomb", other)), other, PopupType.SmallCaution);
    }

    private void OnPreventCollide(Entity<CosmicFieldComponent> ent, ref PreventCollideEvent args)
    {
        if (args.OurFixture.Hard && (!HasComp<StarMarkComponent>(args.OtherEntity) ||
                                     TryComp(args.OtherEntity, out PullableComponent? pullable) &&
                                     (HasComp<StarGazerComponent>(pullable.Puller) ||
                                      TryComp(pullable.Puller, out HereticComponent? heretic) &&
                                      heretic.CurrentPath == "Cosmos")))
            args.Cancelled = true;
    }

    public void SpawnCosmicFieldLine(EntityCoordinates coords,
        DirectionFlag directions,
        int start,
        int end,
        int centerSkipRadius,
        int strength,
        float lifetime = 30f)
    {
        if (start > end)
            return;

        var x = (directions & DirectionFlag.West) != 0 ? -1 : (directions & DirectionFlag.East) != 0 ? 1 : 0;
        var y = (directions & DirectionFlag.South) != 0 ? -1 : (directions & DirectionFlag.North) != 0 ? 1 : 0;

        for (var i = start; i <= end; i++)
        {
            if (centerSkipRadius > 0 && Math.Abs(i) < centerSkipRadius)
                continue;

            SpawnCosmicField(coords.Offset(new Vector2i(x * i, y * i)), strength, lifetime);
        }
    }

    public void SpawnCosmicFields(EntityCoordinates coords, int range, int strength, float lifetime = 30f)
    {
        if (range < 0)
            return;

        for (var y = -range; y <= range; y++)
        {
            for (var x = -range; x <= range; x++)
            {
                SpawnCosmicField(coords.Offset(new Vector2i(x, y)), strength, lifetime);
            }
        }
    }

    public void SpawnCosmicField(EntityCoordinates coords, int strength, float lifetime = 30f)
    {
        if (_net.IsClient)
            return;

        var spawnCoords = coords.SnapToGrid(EntityManager, _mapMan);

        var lookup = _lookup.GetEntitiesInRange<CosmicFieldComponent>(spawnCoords, 0.1f, LookupFlags.Static);
        if (lookup.Count > 0)
        {
            foreach (var (lookEnt, comp) in lookup)
            {
                if (comp.Strength < strength)
                    InitializeCosmicField((lookEnt, comp), strength);

                if (TryComp(lookEnt, out TimedDespawnComponent? despawn) && despawn.Lifetime < lifetime)
                    despawn.Lifetime = lifetime;
            }

            return;
        }

        var ent = Spawn(CosmicField, spawnCoords);
        var xform = Transform(ent);
        _transform.AttachToGridOrMap(ent, xform);
        _transform.AnchorEntity((ent, xform));

        var field = EnsureComp<CosmicFieldComponent>(ent);
        InitializeCosmicField((ent, field), strength);

        EnsureComp<TimedDespawnComponent>(ent).Lifetime = lifetime;
    }

    public void ApplyStarMarkInRange(EntityCoordinates coords, EntityUid? user, float range)
    {
        var ents = _lookup.GetEntitiesInRange<MobStateComponent>(coords, range, LookupFlags.Dynamic);
        foreach (var entity in ents)
        {
            TryApplyStarMark(entity.AsNullable());
        }
    }

    public bool TryApplyStarMark(Entity<MobStateComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false) ||
            TryComp(entity, out HereticComponent? heretic) && heretic.CurrentPath == "Cosmos" ||
            HasComp<GhoulComponent>(entity))
            return false;

        var ev = new BeforeCastTouchSpellEvent(entity, false);
        RaiseLocalEvent(entity, ev, true);

        var result = !ev.Cancelled &&
                     _status.TryUpdateStatusEffectDuration(entity, StarMarkStatusEffect, TimeSpan.FromSeconds(30));

        if (!result)
            return false;

        RegenerateContacts(entity.Owner);
        return true;
    }

    protected virtual void InitializeCosmicField(Entity<CosmicFieldComponent> field, int strength)
    {
        field.Comp.Strength = strength;
        Dirty(field);

        if (strength < 10 || !TryComp(field, out VelocityModifierContactsComponent? modifier))
            return;

        modifier.IsActive = true;
        Dirty(field.Owner, modifier);
    }

    private void RegenerateContacts(Entity<PhysicsComponent?, FixturesComponent?, TransformComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2, ref ent.Comp3, false))
            return;

        _broadphase.RegenerateContacts(ent);
    }
}

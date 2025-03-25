using Content.Shared.Coordinates;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Boomerang;

/// <summary>
/// This system handles boomerang-like behavior to make entities return to the thrower
/// </summary>
public sealed class BoomerangSystem : EntitySystem
{
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BoomerangComponent, LandEvent>(OnLanded);
        SubscribeLocalEvent<BoomerangComponent, ThrownEvent>(OnThrown);

    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var queryEnum = EntityQueryEnumerator<BoomerangComponent>();
        while (queryEnum.MoveNext(out var uid, out var boomerangThrowComponent))
        {
            if (!boomerangThrowComponent.SendBack || _gameTiming.CurTime < boomerangThrowComponent.TimeToReturn || boomerangThrowComponent.Thrower == null)
                continue;
            var inRange = _lookupSystem.GetEntitiesInRange(uid.ToCoordinates(), 3f);
            foreach (var entity in inRange)
            {
                if (entity == boomerangThrowComponent.Thrower)
                    _handsSystem.TryPickup(boomerangThrowComponent.Thrower.Value, uid);
            }
            ReturnToThrower(uid, boomerangThrowComponent);
        }
    }

    private void OnThrown(Entity<BoomerangComponent> ent, ref ThrownEvent args)
    {
        ent.Comp.Thrower = args.User;

        if (ent.Comp.Thrower == null
            || !TryComp<MeleeThrowOnHitComponent>(ent, out var meleeThrowOnHitComponent))
            return;

        meleeThrowOnHitComponent.ActivateOnThrown = true;

    }

    private void OnLanded(Entity<BoomerangComponent> ent, ref LandEvent args)
    {
        if(args.User == null)
            return;
        ent.Comp.TimeToReturn = _gameTiming.CurTime.Add(TimeSpan.FromSeconds(0.25));
        ent.Comp.SendBack = true;
    }

    private void ReturnToThrower(EntityUid uid, BoomerangComponent component)
    {
        if (component.Thrower == null || !Exists(component.Thrower.Value))
            return;

        // Get thrower coordinates
        var throwerCoords = component.Thrower.Value.ToCoordinates();

        // Simply throw it back to the thrower
        if (!TryComp<PhysicsComponent>(uid, out var boomerangPhysicsComponent))
            return;

        _physicsSystem.SetBodyStatus(uid, boomerangPhysicsComponent, BodyStatus.InAir);
        _throwingSystem.TryThrow(uid, throwerCoords, user: component.Thrower.Value);
        if(!TryComp<MeleeThrowOnHitComponent>(uid, out var meleeThrowOnHitComponent))
            return;
        meleeThrowOnHitComponent.ActivateOnThrown = false;
        component.SendBack = false;
    }
}

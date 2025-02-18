using System.Linq;
using Content.Shared._Goobstation.Weapons.SmartGun;
using Content.Shared.Damage.Components;
using Content.Shared.Standing;
using Content.Shared.Wieldable.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Physics;
using Robust.Shared.Enums;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client._Goobstation.Weapons.LaserPointer;

public sealed class LaserPointerOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;
    private readonly IEntityManager _entManager;

    private readonly PhysicsSystem _physics;
    private readonly TransformSystem _transform;

    private readonly ShaderInstance _unshadedShader;

    public LaserPointerOverlay(IEntityManager entManager, IPrototypeManager prototype)
    {
        ZIndex = (int) DrawDepth.Effects;

        _entManager = entManager;

        _physics = entManager.System<PhysicsSystem>();
        _transform = entManager.System<TransformSystem>();

        _unshadedShader = prototype.Index<ShaderPrototype>("unshaded").Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        var xformQuery = _entManager.GetEntityQuery<TransformComponent>();
        var requiresTargetQuery = _entManager.GetEntityQuery<RequireProjectileTargetComponent>();
        var standingQuery = _entManager.GetEntityQuery<StandingStateComponent>();

        var query = _entManager.EntityQueryEnumerator<LaserPointerComponent, WieldableComponent, TransformComponent>();
        handle.UseShader(_unshadedShader);
        while (query.MoveNext(out _, out var pointer, out var wieldable, out var xform))
        {
            if (!wieldable.Wielded)
                continue;

            if (!xformQuery.TryComp(xform.ParentUid, out var parentXform))
                continue;

            var rayLength = 20f;

            // Lying people hit every object even if they are not aiming at it.
            var lying = standingQuery.TryComp(xform.ParentUid, out var standingState) &&
                        standingState.CurrentState != StandingState.Standing;

            var (pos, rot) = _transform.GetWorldPositionRotation(parentXform, xformQuery);
            var dir = pointer.Direction ?? rot.ToWorldVec();

            if (dir.LengthSquared() < 0.0001f)
                continue;

            var normalized = dir.Normalized();

            var ray = new CollisionRay(pos, normalized, pointer.CollisionMask);
            var hit = _physics.IntersectRay(xform.MapID, ray, rayLength, xform.ParentUid, false)
                .OrderBy(x => x.Distance)
                .FirstOrNull(x =>
                    x.HitEntity == pointer.TargetedEntity || lying ||
                    !requiresTargetQuery.TryComp(x.HitEntity, out var requiresTarget) || !requiresTarget.Active);
            if (hit != null)
                rayLength = hit.Value.Distance;

            handle.DrawLine(pos, pos + normalized * rayLength, pointer.TargetedEntity == null ? Color.Red : Color.Green);
        }

        handle.UseShader(null);
    }
}

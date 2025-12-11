using System.Numerics;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Administration;
using Content.Shared.Administration.Managers;
using Content.Shared.Players.PlayTimeTracking;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Server.Administration;

/// <summary>
/// Raises awareness about sedentary lifestyles
/// </summary>
public sealed class FatAdminSystem : EntitySystem
{
    [Dependency] private readonly PlayTimeTrackingManager _playTime = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ISharedAdminManager _admin = default!;

    private const float ScalePerHour = 0.04f;
    private const float MaxScale = 5.0f;
    private const float MinScale = 1.0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        // Even the fucking method is wide
        var session = args.Player;

        if (!_admin.IsAdmin(session))
            return;

        if (!_playTime.TryGetTrackerTime(session, PlayTimeTrackingShared.TrackerAdmin, out var adminTime))
            return;

        var hours = (float)adminTime.Value.TotalHours;

        var scale = MathF.Min(MinScale + (hours * ScalePerHour), MaxScale);

        if (scale <= MinScale + 0.01f)
            return;

        SetFatness(args.Entity, scale);
    }

    private void OnPlayerDetached(PlayerDetachedEvent args)
    {
        if (!_admin.IsAdmin(args.Player))
            return;

        SetFatness(args.Entity, 1f);
        RemCompDeferred<ScaleVisualsComponent>(args.Entity);
    }

    private void SetFatness(EntityUid uid, float scale)
    {
        EnsureComp<ScaleVisualsComponent>(uid);

        var appearanceComponent = EnsureComp<AppearanceComponent>(uid);

        if (!_appearance.TryGetData<Vector2>(uid, ScaleVisuals.Scale, out var oldScale, appearanceComponent))
            oldScale = Vector2.One;

        _appearance.SetData(uid, ScaleVisuals.Scale, new Vector2(scale, oldScale.Y), appearanceComponent);

        if (TryComp(uid, out FixturesComponent? manager))
        {
            var fixtureScale = (scale - oldScale.X) / 4f;

            foreach (var (id, fixture) in manager.Fixtures)
            {
                if (fixture.Shape is PhysShapeCircle circle)
                {
                    _physics.SetPositionRadius(
                        uid,
                        id,
                        fixture,
                        circle,
                        circle.Position,
                        circle.Radius + fixtureScale,
                        manager);
                }
            }
        }
    }
}

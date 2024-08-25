using Content.Shared.Actions;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.DeltaV.Abilities;
using Content.Shared.Maps;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Server.DeltaV.Abilities;

public sealed partial class CrawlUnderObjectsSystem : SharedCrawlUnderObjectsSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movespeed = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawlUnderObjectsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, ToggleCrawlingStateEvent>(OnAbilityToggle);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, AttemptClimbEvent>(OnAttemptClimb);
        SubscribeLocalEvent<CrawlUnderObjectsComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
    }

    private bool IsOnCollidingTile(EntityUid uid)
    {
        var xform = Transform(uid);
        var tile = xform.Coordinates.GetTileRef();
        if (tile == null)
            return false;

        return _turf.IsTileBlocked(tile.Value, CollisionGroup.MobMask);
    }

    private void OnInit(EntityUid uid, CrawlUnderObjectsComponent component, ComponentInit args)
    {
        if (component.ToggleHideAction != null)
            return;

        _actionsSystem.AddAction(uid, ref component.ToggleHideAction, component.ActionProto);
    }

    private bool EnableSneakMode(EntityUid uid, CrawlUnderObjectsComponent component)
    {
        if (component.Enabled
            || (TryComp<ClimbingComponent>(uid, out var climbing)
                && climbing.IsClimbing == true))
            return false;

        component.Enabled = true;
        Dirty(uid, component);
        RaiseLocalEvent(uid, new CrawlingUpdatedEvent(component.Enabled));

        if (TryComp(uid, out FixturesComponent? fixtureComponent))
        {
            foreach (var (key, fixture) in fixtureComponent.Fixtures)
            {
                var newMask = (fixture.CollisionMask
                    & (int)~CollisionGroup.HighImpassable
                    & (int)~CollisionGroup.MidImpassable)
                    | (int)CollisionGroup.InteractImpassable;
                if (fixture.CollisionMask == newMask)
                    continue;

                component.ChangedFixtures.Add((key, fixture.CollisionMask));
                _physics.SetCollisionMask(uid,
                    key,
                    fixture,
                    newMask,
                    manager: fixtureComponent);
            }
        }
        return true;
    }

    private bool DisableSneakMode(EntityUid uid, CrawlUnderObjectsComponent component)
    {
        if (!component.Enabled
            || IsOnCollidingTile(uid)
            || (TryComp<ClimbingComponent>(uid, out var climbing)
                && climbing.IsClimbing == true))
            return false;

        component.Enabled = false;
        Dirty(uid, component);
        RaiseLocalEvent(uid, new CrawlingUpdatedEvent(component.Enabled));

        // Restore normal collision masks
        if (TryComp(uid, out FixturesComponent? fixtureComponent))
            foreach (var (key, originalMask) in component.ChangedFixtures)
                if (fixtureComponent.Fixtures.TryGetValue(key, out var fixture))
                    _physics.SetCollisionMask(uid, key, fixture, originalMask, fixtureComponent);

        component.ChangedFixtures.Clear();
        return true;
    }

    private void OnAbilityToggle(EntityUid uid,
        CrawlUnderObjectsComponent component,
        ToggleCrawlingStateEvent args)
    {
        if (args.Handled)
            return;

        bool result;

        if (component.Enabled)
            result = DisableSneakMode(uid, component);
        else
            result = EnableSneakMode(uid, component);

        if (TryComp<AppearanceComponent>(uid, out var app))
            _appearance.SetData(uid, SneakMode.Enabled, component.Enabled, app);

        _movespeed.RefreshMovementSpeedModifiers(uid);

        args.Handled = result;
    }

    private void OnAttemptClimb(EntityUid uid,
        CrawlUnderObjectsComponent component,
        AttemptClimbEvent args)
    {
        if (component.Enabled == true)
            args.Cancelled = true;
    }

    private void OnRefreshMovespeed(EntityUid uid, CrawlUnderObjectsComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.Enabled)
            args.ModifySpeed(component.SneakSpeedModifier, component.SneakSpeedModifier);
    }
}

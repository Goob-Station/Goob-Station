using Content.Shared.Actions;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.DeltaV.Abilities;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Server.DeltaV.Abilities.Systems;

public sealed partial class HideUnderTableAbilitySystem : SharedHideUnderTableAbilitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private const int HighImpassable = (int) CollisionGroup.HighImpassable;
    private const int MidImpassable = (int) CollisionGroup.MidImpassable;
    private const int InteractImpassable = (int) CollisionGroup.InteractImpassable;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HideUnderTableAbilityComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<HideUnderTableAbilityComponent, ToggleHideUnderTablesEvent>(OnAbilityToggle);
        SubscribeLocalEvent<HideUnderTableAbilityComponent, AttemptClimbEvent>(OnAttemptClimb);
    }

    public void OnInit(EntityUid uid, HideUnderTableAbilityComponent component, ComponentInit args)
    {
        if (component.ToggleHideAction != null)
            return;

        _actionsSystem.AddAction(uid, ref component.ToggleHideAction, component.ActionProto);
    }

    public void EnableSneakMode(EntityUid uid, HideUnderTableAbilityComponent component)
    {
        if (component.Enabled)
            return;

        if (TryComp<ClimbingComponent>(uid, out var climbing) && climbing.IsClimbing == true)
            return;

        component.Enabled = true;
        Dirty(uid, component);
        RaiseLocalEvent(uid, new HideUnderTablesUpdatedEvent(component.Enabled));

        if (TryComp(uid, out FixturesComponent? fixtureComponent))
        {
            foreach (var (key, fixture) in fixtureComponent.Fixtures)
            {
                var newMask = (fixture.CollisionMask & ~HighImpassable & ~MidImpassable) | InteractImpassable;
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
    }

    private void DisableSneakMode(EntityUid uid, HideUnderTableAbilityComponent component)
    {
        if (!component.Enabled)
            return;

        if (TryComp<ClimbingComponent>(uid, out var climbing) && climbing.IsClimbing == true)
            return;

        component.Enabled = false;
        Dirty(uid, component);
        RaiseLocalEvent(uid, new HideUnderTablesUpdatedEvent(component.Enabled));

        // Restore normal collision masks
        if (TryComp(uid, out FixturesComponent? fixtureComponent))
        {
            foreach (var (key, originalMask) in component.ChangedFixtures)
            {
                if (fixtureComponent.Fixtures.TryGetValue(key, out var fixture))
                    _physics.SetCollisionMask(uid, key, fixture, originalMask, fixtureComponent);
            }
        }
        component.ChangedFixtures.Clear();
    }

    public void OnAbilityToggle(EntityUid uid, HideUnderTableAbilityComponent component, ToggleHideUnderTablesEvent args)
    {
        if (component.Enabled)
            DisableSneakMode(uid, component);
        else
            EnableSneakMode(uid, component);

        if (TryComp<AppearanceComponent>(uid, out var app))
            _appearance.SetData(uid, SneakMode.Enabled, component.Enabled, app);
    }

    public void OnAttemptClimb(EntityUid uid, HideUnderTableAbilityComponent component, AttemptClimbEvent args)
    {
        if (component.Enabled == true)
            args.Cancelled = true;
    }
}

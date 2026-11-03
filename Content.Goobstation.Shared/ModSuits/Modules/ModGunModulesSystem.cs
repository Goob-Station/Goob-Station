using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared._Trauma.Projectiles;
using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModGunModulesSystem : VirtualController
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ActionContainerSystem _actionContainer = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedGunSystem _gun = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    private const float FieldExitSlack = 0.5f;

    private readonly HashSet<Entity<ProjectileComponent>> _projectiles = new();
    private readonly List<EntityUid> _released = new();

    [SubscribeLocalEvent]
    private void OnDampenerActivated(Entity<ModProjectileDampenerModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (_timing.ApplyingState || args.Wearer is not { } wearer)
            return;

        var field = EnsureComp<ModDampenerFieldComponent>(wearer);
        field.Module = ent;
        field.Radius = ent.Comp.Radius;
        field.SpeedMultiplier = ent.Comp.SpeedMultiplier;
        field.DamageMultiplier = ent.Comp.DamageMultiplier;
        Dirty(wearer, field);
    }

    [SubscribeLocalEvent]
    private void OnDampenerDeactivated(Entity<ModProjectileDampenerModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        RemoveField(ent);
    }

    [SubscribeLocalEvent]
    private void OnDampenerWearerChanged(Entity<ModProjectileDampenerModuleComponent> ent, ref ModSuitWearerChangedEvent args)
    {
        if (args.OldWearer is { } old && TryComp<ModDampenerFieldComponent>(old, out var field) && field.Module == ent.Owner)
            RemCompDeferred(old, field);
    }

    private void RemoveField(EntityUid module)
    {
        if (_timing.ApplyingState)
            return;

        var query = EntityQueryEnumerator<ModDampenerFieldComponent>();
        while (query.MoveNext(out var uid, out var field))
        {
            if (field.Module == module)
                RemCompDeferred(uid, field);
        }
    }

    [SubscribeLocalEvent]
    private void OnDampenedHit(Entity<ModDampenedProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        args.Damage *= ent.Comp.DamageMultiplier;
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        var fields = EntityQueryEnumerator<ModDampenerFieldComponent, TransformComponent>();
        while (fields.MoveNext(out var wearer, out var field, out var xform))
        {
            _projectiles.Clear();
            _lookup.GetEntitiesInRange(xform.Coordinates, field.Radius, _projectiles);
            foreach (var projectile in _projectiles)
            {
                if (HasComp<ModDampenedProjectileComponent>(projectile)
                    || _container.IsEntityInContainer(projectile)
                    || !TryComp<PhysicsComponent>(projectile, out var body)
                    || prediction && !body.Predict)
                    continue;

                var speed = body.LinearVelocity.Length();
                if (speed < 0.01f)
                    continue;

                var dampened = EnsureComp<ModDampenedProjectileComponent>(projectile);
                dampened.Field = wearer;
                dampened.OriginalSpeed = speed;
                dampened.DamageMultiplier = field.DamageMultiplier;
                Dirty(projectile, dampened);

                PhysicsSystem.SetLinearVelocity(projectile, body.LinearVelocity * field.SpeedMultiplier, body: body);
            }
        }

        _released.Clear();
        var tracked = EntityQueryEnumerator<ModDampenedProjectileComponent, PhysicsComponent, TransformComponent>();
        while (tracked.MoveNext(out var uid, out var dampened, out var body, out var xform))
        {
            if (prediction && !body.Predict)
                continue;

            var velocity = body.LinearVelocity;
            var speed = velocity.Length();
            var inside = TryComp<ModDampenerFieldComponent>(dampened.Field, out var field)
                && TransformSystem.InRange(xform.Coordinates, Transform(dampened.Field).Coordinates, field.Radius + FieldExitSlack);

            if (!inside)
            {
                if (speed > 0.001f)
                    PhysicsSystem.SetLinearVelocity(uid, velocity / speed * dampened.OriginalSpeed, body: body);

                _released.Add(uid);
                continue;
            }

            var target = dampened.OriginalSpeed * field!.SpeedMultiplier;
            if (speed > 0.001f && MathF.Abs(speed - target) > 0.01f)
                PhysicsSystem.SetLinearVelocity(uid, velocity / speed * target, body: body);
        }

        foreach (var uid in _released)
        {
            RemComp<ModDampenedProjectileComponent>(uid);
        }
    }

    [SubscribeLocalEvent]
    private void OnAssistantMapInit(Entity<ModShootingAssistantModuleComponent> ent, ref MapInitEvent args)
    {
        if (!_actionContainer.EnsureAction(ent, ref ent.Comp.CycleActionEntity, ent.Comp.CycleAction))
            return;

        _actions.SetEntityIcon(ent.Comp.CycleActionEntity.Value, ent.Owner);
        Dirty(ent);
    }

    [SubscribeLocalEvent]
    private void OnAssistantWearerChanged(Entity<ModShootingAssistantModuleComponent> ent, ref ModSuitWearerChangedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (args.OldWearer is { } old)
        {
            if (_actions.GetAction(ent.Comp.CycleActionEntity, false) is { } action && action.Comp.AttachedEntity == old)
                _actions.RemoveProvidedAction(old, ent, action);

            RemoveAssistant(ent, old);
        }

        if (args.NewWearer is { } wearer && _actions.GetAction(ent.Comp.CycleActionEntity, false) is { } newAction)
            _actions.AddAction(wearer, newAction.AsNullable(), ent.Owner);
    }

    [SubscribeLocalEvent]
    private void OnAssistantInstalled(Entity<ModShootingAssistantModuleComponent> ent, ref ModModuleInstalledEvent args)
    {
        if (!_timing.ApplyingState
            && TryComp<ModSuitComponent>(args.Suit, out var suit)
            && suit.Wearer is { } wearer
            && _actions.GetAction(ent.Comp.CycleActionEntity, false) is { } action)
            _actions.AddAction(wearer, action.AsNullable(), ent.Owner);
    }

    [SubscribeLocalEvent]
    private void OnAssistantRemoved(Entity<ModShootingAssistantModuleComponent> ent, ref ModModuleRemovedEvent args)
    {
        if (_timing.ApplyingState
            || _actions.GetAction(ent.Comp.CycleActionEntity, false) is not { } action
            || action.Comp.AttachedEntity is not { } user)
            return;

        _actions.RemoveProvidedAction(user, ent, action);
        RemoveAssistant(ent, user);
    }

    [SubscribeLocalEvent]
    private void OnAssistantActivated(Entity<ModShootingAssistantModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (args.Wearer is { } wearer)
            ApplyAssistant(ent, wearer);
    }

    [SubscribeLocalEvent]
    private void OnAssistantDeactivated(Entity<ModShootingAssistantModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (args.Wearer is { } wearer)
            RemoveAssistant(ent, wearer);
    }

    [SubscribeLocalEvent]
    private void OnCycleMode(Entity<ModShootingAssistantModuleComponent> ent, ref ModShootingModeActionEvent args)
    {
        if (args.Handled || !_modSuit.TryGetModuleSuit(ent, out var suit) || suit.Comp.Wearer != args.Performer)
            return;

        args.Handled = true;
        ent.Comp.Mode = ent.Comp.Mode switch
        {
            ModShootingMode.Off => ModShootingMode.Stormtrooper,
            ModShootingMode.Stormtrooper => ModShootingMode.Sharpshooter,
            _ => ModShootingMode.Off,
        };
        Dirty(ent);

        _popup.PopupClient(Loc.GetString($"modsuit-shooting-mode-{ent.Comp.Mode.ToString().ToLowerInvariant()}"), args.Performer, args.Performer);

        if (TryComp<ModModuleComponent>(ent, out var module) && module.Active)
            ApplyAssistant(ent, args.Performer);
    }

    private void ApplyAssistant(Entity<ModShootingAssistantModuleComponent> ent, EntityUid wearer)
    {
        if (_timing.ApplyingState)
            return;

        var assistant = EnsureComp<ModShootingAssistantComponent>(wearer);
        assistant.Module = ent;
        assistant.Mode = ent.Comp.Mode;
        Dirty(wearer, assistant);
        RefreshShooter(wearer);
    }

    private void RemoveAssistant(Entity<ModShootingAssistantModuleComponent> ent, EntityUid wearer)
    {
        if (_timing.ApplyingState
            || TerminatingOrDeleted(wearer)
            || !TryComp<ModShootingAssistantComponent>(wearer, out var assistant)
            || assistant.Module != ent.Owner)
            return;

        assistant.Mode = ModShootingMode.Off;
        RefreshShooter(wearer);
        RemCompDeferred(wearer, assistant);
    }

    private void RefreshShooter(EntityUid wearer)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(wearer);
        foreach (var held in _hands.EnumerateHeld(wearer))
        {
            if (TryComp<GunComponent>(held, out var gun))
                _gun.RefreshModifiers((held, gun), wearer);
        }
    }

    [SubscribeLocalEvent(after: [typeof(SharedWieldableSystem)])]
    private void OnAssistantGunRefresh(Entity<GunComponent> ent, ref GunRefreshModifiersEvent args)
    {
        if (!TryComp<ModShootingAssistantComponent>(ent.Comp.Holder, out var assistant)
            || !TryComp<ModShootingAssistantModuleComponent>(assistant.Module, out var module))
            return;

        switch (assistant.Mode)
        {
            case ModShootingMode.Stormtrooper:
                args.FireRate *= module.StormtrooperFireRate;
                args.MinAngle += Angle.FromDegrees(module.StormtrooperMinSpread);
                args.MaxAngle += Angle.FromDegrees(module.StormtrooperMaxSpread);
                break;
            case ModShootingMode.Sharpshooter:
                args.MinAngle *= module.SharpshooterSpreadMultiplier;
                args.MaxAngle *= module.SharpshooterSpreadMultiplier;
                args.AngleIncrease *= module.SharpshooterSpreadMultiplier;
                args.ProjectileSpeed *= module.SharpshooterProjectileSpeed;
                break;
        }
    }

    [SubscribeLocalEvent]
    private void OnAssistantSpeed(Entity<ModShootingAssistantComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Mode != ModShootingMode.Sharpshooter
            || !TryComp<ModShootingAssistantModuleComponent>(ent.Comp.Module, out var module))
            return;

        args.ModifySpeed(module.SharpshooterMoveSpeed, module.SharpshooterMoveSpeed);
    }

    [SubscribeLocalEvent]
    private void OnPlayerShot(ref PlayerShotProjectileEvent args)
    {
        if (!TryComp<ModShootingAssistantComponent>(args.User, out var assistant)
            || assistant.Mode != ModShootingMode.Sharpshooter
            || !TryComp<ModShootingAssistantModuleComponent>(assistant.Module, out var module)
            || !TryComp<ProjectileComponent>(args.Projectile, out var projectile))
            return;

        projectile.Damage *= module.SharpshooterDamage;
    }
}

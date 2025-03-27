using System.Numerics;
using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.EntitySystems;
using Content.Shared._Goobstation.Medical;
using Content.Shared._Goobstation.Medical.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Physics;
using Content.Shared.Timing;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Medical;

// TODO: Move this to Shared when battery systems will be predicted
public sealed class MedigunSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    private EntityQuery<DamageableComponent> _damageableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();

        SubscribeLocalEvent<MediGunComponent, AfterInteractEvent>(OnActivate);
        SubscribeLocalEvent<MediGunComponent, MediGunUberActivateActionEvent>(OnUber);
        SubscribeLocalEvent<MediGunComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<MediGunComponent, ItemToggledEvent>(OnToggled);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MediGunComponent>();
        while (query.MoveNext(out var medical, out var component))
        {
            if (!component.IsActive)
                continue;

            if (_timing.CurTime < component.NextTick || component.NextTick == null)
                continue;

            var medGunEnt = (medical, component);
            if (_timing.CurTime > component.UberEndTime && component.UberEndTime != null)
            {
                DisableUber(medGunEnt);
            }

            var toHeal = component.HealedEntities.ToArray();
            foreach (var healed in toHeal)
            {
                if (!MediGunHealingTick(medGunEnt, healed))
                    DisableConnection(medGunEnt, healed);
            }

            if (component.HealedEntities.Count == 0)
            {
                DisableAllConnections(medGunEnt);
                continue;
            }

            component.NextTick = _timing.CurTime + TimeSpan.FromSeconds(component.Frequency);

            // Add uber action if we can
            if (component.UberPoints > component.PointsToUber &&
                component.ParentEntity != null &&
                !component.UberActivated)
            {
                _action.AddAction(component.ParentEntity.Value, ref component.UberAction, component.UberActionId, medical);
            }
        }
    }

    /// <summary>
    /// Returns false if target had failed to be healed.
    /// </summary>
    private bool MediGunHealingTick(Entity<MediGunComponent> ent, EntityUid healed)
    {
        var comp = ent.Comp;

        // Calculate positions of all targets and remove ones that out of range
        var healedPos = _xform.GetMapCoordinates(healed);
        var mediGunPos = _xform.GetMapCoordinates(ent);
        var distance = (mediGunPos.Position - healedPos.Position).Length();

        if (distance > comp.MaxRange ||
            healedPos.MapId != mediGunPos.MapId)
        {
            // Disable
            return false;
        }

        var batteryToWithdraw = comp.UberActivated ? comp.UberBatteryWithdraw: comp.BatteryWithdraw;
        if (!_battery.TryUseCharge(ent, batteryToWithdraw))
        {
            _battery.SetCharge(ent, 0f); // because it works wonky
            return false;
        }

        // Do the damage (heal)
        if (!_damageableQuery.TryComp(healed, out var damageable))
            return false;

        // If we're under an uber, heal like it's activated
        var healing = comp.UberActivated ? comp.UberHealing : comp.Healing;
        var originalDamage = damageable.TotalDamage;

        _damage.TryChangeDamage(
            healed,
            healing,
            true,
            false,
            damageable,
            ent.Comp.ParentEntity,
            false,
            partMultiplier: 1.0f,
            targetPart: TargetBodyPart.All);

        _bloodstreamSystem.TryModifyBleedAmount(healed, comp.BleedingModifier);

        var afterDamage = damageable.TotalDamage;
        var healedAmount = originalDamage - afterDamage;

        if (!comp.UberActivated)
            comp.UberPoints += healedAmount;

        return true;
    }

    private void OnToggled(Entity<MediGunComponent> ent, ref ItemToggledEvent args)
    {
        // Player should pick the target by interacting with it.
        if (args.Activated)
            return;

        // Handle disabling
        DisableAllConnections(ent);
    }

    private void OnActivate(EntityUid uid, MediGunComponent component, AfterInteractEvent args)
    {
        if (args.Handled || args.Target == null || args.Target.Value == args.User)
            return;

        if (_useDelay.IsDelayed(uid))
            return;

        if (component.HealedEntities.Count >= component.MaxLinksAmount)
            return;

        if (!_toggle.TryActivate(uid, args.User))
        {
            return;
        }

        var target = args.Target.Value;

        if (!_whitelist.IsWhitelistPass(component.HealAbleWhitelist, target) ||
            component.HealedEntities.Contains(target))
            return;

        if (HasComp<MediGunHealedComponent>(target))
        {
            // boom
            _explosion.QueueExplosion(uid, "Default", 20, 3, 3.4f, 1f, 0, false, args.User);
            QueueDel(uid);
            return;
        }

        component.HealedEntities.Add(target);
        component.IsActive = true;
        component.ParentEntity = args.User;
        component.NextTick = _timing.CurTime + TimeSpan.FromSeconds(component.Frequency);
        Dirty(uid, component);

        // This is used for beam visuals.
        var dummyBeamVisual = Spawn("TetherEntity", new EntityCoordinates(target, Vector2.Zero));

        var mediGunned = EnsureComp<MediGunHealedComponent>(target);
        mediGunned.DummyEntity = dummyBeamVisual;
        mediGunned.Source = uid;
        Dirty(target, mediGunned);

        var visuals = EnsureComp<JointVisualsComponent>(uid);
        visuals.Sprite = component.BeamSprite;
        visuals.OffsetA = new Vector2(0f, 0f);
        visuals.Target = GetNetEntity(dummyBeamVisual);
        Dirty(uid, visuals);

        _useDelay.TryResetDelay(uid);
        args.Handled = true;
    }

    private void OnParentChanged(EntityUid uid, MediGunComponent component, ref EntParentChangedMessage args)
    {
        if (args.Transform.ParentUid == component.ParentEntity)
            return;

        // Disable our gun
        DisableAllConnections((uid, component));
    }

    private void OnUber(EntityUid uid, MediGunComponent component, MediGunUberActivateActionEvent args)
    {
        EnableUber((uid, component));
    }

    /// <summary>
    /// Activates uber mode for this medigun and changes all visuals.
    /// </summary>
    private void EnableUber(Entity<MediGunComponent> ent)
    {
        var comp = ent.Comp;
        comp.UberActivated = true;
        comp.UberEndTime = _timing.CurTime + TimeSpan.FromSeconds(comp.UberDefaultLenght);
        comp.UberPoints = 0;
        _action.RemoveAction(comp.UberAction);
        Dirty(ent);

        var visuals = EnsureComp<JointVisualsComponent>(ent);
        visuals.Sprite = comp.UberBeamSprite;
        Dirty(ent, visuals);
    }

    /// <summary>
    /// Removes all uber related values and restores normal visuals.
    /// </summary>
    private void DisableUber(Entity<MediGunComponent> ent)
    {
        var comp = ent.Comp;
        comp.UberActivated = false;
        comp.UberEndTime = null;
        Dirty(ent);

        var visuals = EnsureComp<JointVisualsComponent>(ent);
        visuals.Sprite = comp.BeamSprite;
        Dirty(ent, visuals);
    }

    /// <summary>
    /// Handles removing all connections from medigun when it's disabling.
    /// Also does the full job with disabling medigun.
    /// </summary>
    private void DisableAllConnections(Entity<MediGunComponent> ent)
    {
        var comp = ent.Comp;
        foreach (var healed in comp.HealedEntities)
        {
            if (!TryComp<MediGunHealedComponent>(healed, out var mediGunned))
                return;

            QueueDel(mediGunned.DummyEntity);
            mediGunned.DummyEntity = null;
            RemComp(healed, mediGunned);
        }

        _toggle.TryDeactivate(ent.Owner, comp.ParentEntity);

        comp.HealedEntities.Clear();
        comp.IsActive = false;
        comp.ParentEntity = null;
        RemComp<JointVisualsComponent>(ent);
    }

    /// <summary>
    /// Disables a connection to a specific entity. Also removes it from HealedEntities list.
    /// </summary>
    private void DisableConnection(Entity<MediGunComponent> ent, EntityUid toRemove)
    {
        var comp = ent.Comp;
        if (!comp.HealedEntities.Contains(toRemove))
            return;

        if (!TryComp<MediGunHealedComponent>(toRemove, out var mediGunned))
            return;

        QueueDel(mediGunned.DummyEntity);
        RemComp(toRemove, mediGunned);
        comp.HealedEntities.Remove(toRemove);
    }
}

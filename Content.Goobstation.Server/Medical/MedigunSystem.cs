// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Medical;
using Content.Goobstation.Shared.Medical.Components;
using Content.Goobstation.Common.ContinuousBeam;
using Content.Server.Body.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.EntitySystems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Server.Examine;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Physics;
using Content.Shared.Timing;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;
namespace Content.Goobstation.Server.Medical;

// TODO: Move this to Shared when battery systems will be predicted
public sealed class MedigunSystem : SharedMedigunSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly AlertsSystem _alert = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly ExamineSystem _examine = default!;

    private EntityQuery<DamageableComponent> _damageableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();

        SubscribeLocalEvent<MediGunComponent, AfterInteractEvent>(OnActivate);
        SubscribeLocalEvent<MediGunComponent, MediGunUberActivateActionEvent>(OnUber);
        SubscribeLocalEvent<MediGunComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<MediGunComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<ContinuousBeamComponent, MedigunBeamEvent>(OnBeamTick);
    }

    private void OnBeamTick(Entity<ContinuousBeamComponent> ent, ref MedigunBeamEvent args)
    {
        if (!TryGetEntity(args.User, out var user) || ent.Owner != user.Value)
            return;

        args.Handled = true;

        if (!TryGetEntity(args.Target, out var target) || !TryComp(target.Value, out TransformComponent? targetXform) ||
            !targetXform.Coordinates.IsValid(EntityManager))
        {
            BreakBeam(args.Target);
            return;
        }

        var coords = _xform.GetMapCoordinates(user.Value);
        var targetCoords = _xform.GetMapCoordinates(target.Value, targetXform);

        if (coords.MapId != targetCoords.MapId)
        {
            BreakBeam(args.Target);
            return;
        }

        if (!TryComp<MediGunComponent>(user.Value, out var medigun))
        {
            BreakBeam(args.Target);
            return;
        }

        // Check if target is in range and not occluded
        if ((coords.Position - targetCoords.Position).Length() > medigun.MaxRange ||
            !_examine.InRangeUnOccluded(user.Value, target.Value))
        {
            BreakBeam(args.Target);
            return;
        }

        // Apply healing based on medigun settings
        var healing = medigun.UberActivated ? medigun.UberHealing : medigun.Healing;

        if (args.Healing != null)
            healing = args.Healing;

        // Apply healing to target
        if (_damageableQuery.TryComp(target.Value, out var damageable))
        {
            var originalDamage = damageable.TotalDamage;

            _damage.TryChangeDamage(
                target.Value,
                healing,
                true,
                false,
                damageable,
                medigun.ParentEntity,
                targetPart: TargetBodyPart.All);

            _bloodstreamSystem.TryModifyBleedAmount(target.Value, medigun.BleedingModifier);

            var afterDamage = damageable.TotalDamage;
            var healedAmount = originalDamage - afterDamage;

            // Track uber points if healing was successful
            if (!medigun.UberActivated)
                medigun.UberPoints += healedAmount.Float();
        }

        return;

        void BreakBeam(NetEntity netTarget)
        {
            ent.Comp.Data.Remove(netTarget);
            if (ent.Comp.Data.Count == 0)
                RemCompDeferred(ent.Owner, ent.Comp);
            else
                Dirty(ent);
        }
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
            comp.UberPoints += healedAmount.Float();

        if (comp.ParentEntity != null)
            UpdateAlert(comp.ParentEntity.Value, ent);

        return true;
    }

    private void OnToggled(Entity<MediGunComponent> ent, ref ItemToggledEvent args)
    {
        if (ent.Comp.ParentEntity != null)
            UpdateAlert(ent.Comp.ParentEntity.Value, ent);

        // Player should pick the target by interacting with it.
        if (args.Activated)
            return;

        // Handle disabling
        DisableAllConnections(ent);
    }

    private void OnActivate(Entity<MediGunComponent> ent, ref AfterInteractEvent args)
    {
        var (uid, comp) = ent;

        if (args.Target == null || args.Target.Value == args.User)
            return;

        if (_useDelay.IsDelayed(uid))
            return;

        if (comp.HealedEntities.Count >= comp.MaxLinksAmount)
            return;

        var target = args.Target.Value;

        if (!_whitelist.IsWhitelistPass(comp.HealAbleWhitelist, target) ||
            comp.HealedEntities.Contains(target))
            return;

        if (HasComp<MediGunHealedComponent>(target))
        {
            // boom
            _explosion.QueueExplosion(uid, "Default", 20, 3, 3.4f, 1f, 0, false, args.User);
            QueueDel(uid);
            return;
        }

        if (!_toggle.TryActivate(uid, args.User))
        {
            return;
        }

        _audio.PlayPvs(comp.SoundOnTarget, uid);

        comp.HealedEntities.Add(target);
        comp.IsActive = true;
        comp.ParentEntity = args.User;
        comp.NextTick = _timing.CurTime + TimeSpan.FromSeconds(comp.Frequency);
        Dirty(uid, comp);

        // This is used for beam visuals.
        var dummyBeamVisual = Spawn("TetherEntity", new EntityCoordinates(target, Vector2.Zero));

        var mediGunned = EnsureComp<MediGunHealedComponent>(target);
        mediGunned.DummyEntity = dummyBeamVisual;
        mediGunned.Source = uid;
        mediGunned.LineColor = comp.UberActivated ? comp.UberLineColor : comp.DefaultLineColor;
        Dirty(target, mediGunned);

        // Add continuous beam component
        var beam = EnsureComp<ContinuousBeamComponent>(args.User);
        var netTarget = GetNetEntity(target);
        beam.Data.Remove(netTarget);

        // Use appropriate healing based on uber status
        var healing = comp.UberActivated ? comp.UberHealing : comp.Healing;

        beam.Data.Add(netTarget,
            new ContinuousBeamData(
            comp.UberActivated ? comp.UberBeamSprite : comp.BeamSprite,
            comp.Frequency * 2, // Slightly longer than update frequency
            comp.Frequency,
            comp.MaxRange * comp.MaxRange,
            comp.UberActivated ? comp.UberLineColor : comp.DefaultLineColor,
            new MedigunBeamEvent { Healing = healing }));
        Dirty(args.User, beam);
        UpdateAlert(target, ent);
        _useDelay.TryResetDelay(uid);
        args.Handled = true;
    }

    private void UpdateAlert(EntityUid target, Entity<MediGunComponent> medigun)
    {
        var comp = medigun.Comp;
        var parent = Transform(medigun).ParentUid;

        if (parent != comp.ParentEntity ||
            !_toggle.IsActivated(medigun.Owner))
        {
            _alert.ClearAlert(target, "MedigunUberBattery");
            return;
        }

        var severity = (short) MathF.Round(comp.UberPoints / comp.PointsToUber * 10f);
        const short minSeverity = 0;
        const short maxSeverity = 10;
        severity = Math.Clamp(severity, minSeverity, maxSeverity);

        if (comp.UberActivated)
            severity = 11;

        _alert.ShowAlert(target, "MedigunUberBattery", severity);
    }

    private void OnParentChanged(Entity<MediGunComponent> ent, ref EntParentChangedMessage args)
    {
        if (args.Transform.ParentUid == ent.Comp.ParentEntity)
            return;

        if (args.OldParent != null)
            UpdateAlert(args.OldParent.Value, ent);

        // Disable our gun
        DisableAllConnections(ent);
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

        _audio.PlayPvs(comp.SoundOnTarget, ent);
        comp.UberActivated = true;
        comp.UberEndTime = _timing.CurTime + TimeSpan.FromSeconds(comp.UberDefaultLenght);
        comp.UberPoints = 0;
        _action.RemoveAction(comp.UberAction);
        Dirty(ent);

        var visuals = EnsureComp<JointVisualsComponent>(ent);
        visuals.Sprite = comp.UberBeamSprite;
        Dirty(ent, visuals);

        // Update beam for each target
        foreach (var healed in comp.HealedEntities)
        {
            if (!TryComp<MediGunHealedComponent>(healed, out var healComp))
                continue;

            healComp.LineColor = comp.UberLineColor;
            Dirty(healed, healComp);

            // Update continuous beam if exists
            if (TryComp<ContinuousBeamComponent>(comp.ParentEntity, out var beam))
            {
                var netTarget = GetNetEntity(healed);
                if (beam.Data.TryGetValue(netTarget, out var data))
                {
                    beam.Data[netTarget] = new ContinuousBeamData(
                        comp.UberBeamSprite,
                        data.Lifetime,
                        data.TickInterval,
                        data.MaxDistanceSquared,
                        comp.UberLineColor,
                        new MedigunBeamEvent { Healing = comp.UberHealing });
                    Dirty(comp.ParentEntity.Value, beam);
                }
            }
        }
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

        foreach (var healed in comp.HealedEntities)
        {
            if (!TryComp<MediGunHealedComponent>(healed, out var healComp))
                continue;

            healComp.LineColor = comp.DefaultLineColor;
            Dirty(healed, healComp);
        }
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

        if (comp.ParentEntity != null)
            UpdateAlert(comp.ParentEntity.Value, ent);
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

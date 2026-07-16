// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.Inventory;
using Content.Server.Mind.Commands;
using Content.Server.Polymorph.Components;
using Content.Shared._DV.Polymorph;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NameModifier.Components;
using Content.Shared.Nutrition;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

// Shitmed Change
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Body;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using System.Linq;

namespace Content.Server.Polymorph.Systems;

public sealed partial class PolymorphSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!; // Goobstation
    [Dependency] private readonly ISerializationManager _serialization = default!; // Goobstation
    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly TagSystem _tag = default!; // goob edit

    // Shitmed Deps
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly WoundSystem _wound = default!;

    private const string RevertPolymorphId = "ActionRevertPolymorph";

    public override void Initialize()
    {
        SubscribeLocalEvent<PolymorphableComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<PolymorphedEntityComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<PolymorphableComponent, PolymorphActionEvent>(OnPolymorphActionEvent);
        SubscribeLocalEvent<PolymorphedEntityComponent, RevertPolymorphActionEvent>(OnRevertPolymorphActionEvent);

        SubscribeLocalEvent<PolymorphedEntityComponent, BeforeFullySlicedEvent>(OnBeforeFullySliced);
        SubscribeLocalEvent<PolymorphedEntityComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<PolymorphedEntityComponent, EntityTerminatingEvent>(OnPolymorphedTerminating);

        InitializeMap();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PolymorphedEntityComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.Time += frameTime;

            if (comp.Configuration.Duration != null && comp.Time >= comp.Configuration.Duration)
            {
                Revert((uid, comp));
                continue;
            }

            if (!TryComp<MobStateComponent>(uid, out var mob))
                continue;

            if (comp.Configuration.RevertOnDeath && _mobState.IsDead(uid, mob) ||
                comp.Configuration.RevertOnCrit && _mobState.IsIncapacitated(uid, mob))
            {
                Revert((uid, comp));
            }
        }
    }

    private void OnComponentStartup(Entity<PolymorphableComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.InnatePolymorphs != null)
        {
            foreach (var morph in ent.Comp.InnatePolymorphs)
            {
                CreatePolymorphAction(morph, ent);
            }
        }
    }

    private void OnMapInit(Entity<PolymorphedEntityComponent> ent, ref MapInitEvent args)
    {
        var (uid, component) = ent;
        if (component.Configuration.Forced)
            return;

        if (_actions.AddAction(uid, ref component.Action, out var action, RevertPolymorphId))
        {
            _actions.SetEntityIcon((component.Action.Value, action), component.Parent);
            _actions.SetUseDelay(component.Action.Value, TimeSpan.FromSeconds(component.Configuration.Delay));
            if (component.Configuration.SkipRevertConfirmation) // Goobstation
                RemComp<ConfirmableActionComponent>(component.Action.Value);
        }
    }

    private void OnPolymorphActionEvent(Entity<PolymorphableComponent> ent, ref PolymorphActionEvent args)
    {
        if (!_proto.Resolve(args.ProtoId, out var prototype) || args.Handled)
            return;

        PolymorphEntity(ent, prototype.Configuration);

        args.Handled = true;
    }

    private void OnRevertPolymorphActionEvent(Entity<PolymorphedEntityComponent> ent,
        ref RevertPolymorphActionEvent args)
    {
        Revert((ent, ent));
    }

    private void OnBeforeFullySliced(Entity<PolymorphedEntityComponent> ent, ref BeforeFullySlicedEvent args)
    {
        if (ent.Comp.Reverted || !ent.Comp.Configuration.RevertOnEat)
            return;

        args.Cancel();
        Revert((ent, ent));
    }

    /// <summary>
    /// It is possible to be polymorphed into an entity that can't "die", but is instead
    /// destroyed. This handler ensures that destruction is treated like death.
    /// </summary>
    private void OnDestruction(Entity<PolymorphedEntityComponent> ent, ref DestructionEventArgs args)
    {
        if (ent.Comp.Reverted || !ent.Comp.Configuration.RevertOnDeath)
            return;

        Revert((ent, ent));
    }

    private void OnPolymorphedTerminating(Entity<PolymorphedEntityComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.Reverted)
            return;

        if (ent.Comp.Configuration.RevertOnDelete)
            Revert(ent.AsNullable());

        // Remove our original entity too
        // Note that Revert will set Parent to null, so reverted entities will not be deleted
        QueueDel(ent.Comp.Parent);
    }

    /// <summary>
    /// Polymorphs the target entity into the specific polymorph prototype
    /// </summary>
    /// <param name="uid">The entity that will be transformed</param>
    /// <param name="protoId">The id of the polymorph prototype</param>
    public EntityUid? PolymorphEntity(EntityUid uid, ProtoId<PolymorphPrototype> protoId)
    {
        var config = _proto.Index(protoId).Configuration;
        return PolymorphEntity(uid, config);
    }

    /// <summary>
    /// Polymorphs the target entity into another.
    /// </summary>
    /// <param name="uid">The entity that will be transformed</param>
    /// <param name="configuration">The new polymorph configuration</param>
    /// <returns>The new entity, or null if the polymorph failed.</returns>
    public EntityUid? PolymorphEntity(EntityUid uid, PolymorphConfiguration configuration)
    {
        // If they're morphed, check their current config to see if they can be
        // morphed again
        if (!configuration.IgnoreAllowRepeatedMorphs
            && TryComp<PolymorphedEntityComponent>(uid, out var currentPoly)
            && !currentPoly.Configuration.AllowRepeatedMorphs)
            return null;

        // If this polymorph has a cooldown, check if that amount of time has passed since the
        // last polymorph ended.
        if (TryComp<PolymorphableComponent>(uid, out var polymorphableComponent) &&
            polymorphableComponent.LastPolymorphEnd != null &&
            _gameTiming.CurTime < polymorphableComponent.LastPolymorphEnd + configuration.Cooldown)
            return null;

        // mostly just for vehicles
        if (TryComp(uid, out BuckleComponent? buckle)) // Goob edit
            _buckle.TryUnbuckle((uid, buckle), uid, true);

        var targetTransformComp = Transform(uid);

        if (configuration.PolymorphSound != null)
            _audio.PlayPvs(configuration.PolymorphSound, targetTransformComp.Coordinates);

        // Goob edit start
        var proto = configuration.Entity;
        if (proto == null)
        {
            if (!_proto.TryIndex(configuration.Entities, out var entities) || entities.Weights.Count == 0)
            {
                if (!_proto.TryIndex(configuration.Groups, out var groups) || groups.Weights.Count == 0)
                    return null;

                var weightedEntityRandom = groups.Pick(_random);
                if (!_proto.TryIndex(weightedEntityRandom, out entities) || entities.Weights.Count == 0)
                    return null;
            }

            proto = entities.Pick(_random);
        }
        var child = Spawn(proto, _transform.GetMapCoordinates(uid, targetTransformComp), rotation: _transform.GetWorldRotation(uid));

        _mindSystem.MakeSentient(child, configuration.AllowMovement);
        // Goob edit end

        var polymorphedComp = Factory.GetComponent<PolymorphedEntityComponent>();
        polymorphedComp.Parent = uid;
        polymorphedComp.Configuration = configuration;
        AddComp(child, polymorphedComp);

        var childXform = Transform(child);
        _transform.SetLocalRotation(child, targetTransformComp.LocalRotation, childXform);

        // Goob edit start
        if (configuration.AttachToGridOrMap)
            _transform.AttachToGridOrMap(child, childXform);
        else if (_container.TryGetContainingContainer((uid, targetTransformComp, null), out var cont))
            _container.Insert(child, cont);
        // Goob edit end

        //Transfers all damage from the original to the new one
        if (configuration.TransferDamage &&
            TryComp<DamageableComponent>(child, out var damageChild) &&
            _mobThreshold.GetScaledDamage(uid, child, out var damage, out var woundableDamage) &&
            damage != null)
        {
            if (TryComp<BodyComponent>(child, out var childBody)
                && childBody.BodyType == Shared._Shitmed.Body.BodyType.Complex // Too lazy to come up with a new name lmfao
                && _body.TryGetRootPart(child, out var rootPart, childBody))
            {
                var woundables = _wound.GetAllWoundableChildrenWithComp<DamageableComponent>(rootPart.Value);
                var count = woundables.Count();
                foreach (var woundable in woundables)
                {
                    var target = _body.GetTargetBodyPart(woundable);

                    if (woundableDamage is not null)
                    {
                        if (woundableDamage.TryGetValue(target, out var wounds))
                            _damageable.SetDamage(woundable, woundable.Comp2, wounds);
                    }
                    else
                    {
                        _damageable.SetDamage(woundable, woundable.Comp2, damage / count);
                    }
                }

            }
            _damageable.SetDamage(child, damageChild, damage);
        }

        // DeltaV - Drop MindContainer entities on polymorph
        var beforePolymorphedEv = new BeforePolymorphedEvent();
        RaiseLocalEvent(uid, ref beforePolymorphedEv);

        if (configuration.Inventory == PolymorphInventoryChange.Transfer)
        {
            // Goob edit start
            if (TryComp(uid, out InventoryComponent? inventory1))
            {
                if (TryComp(child, out InventoryComponent? inventory2))
                {
                    _inventory.TransferEntityInventories((uid, inventory1), (child, inventory2), false);
                    foreach (var hand in _hands.EnumerateHeld(uid))
                    {
                        _hands.TryDrop(uid, hand, checkActionBlocker: false);
                        _hands.TryPickupAnyHand(child, hand);
                    }
                }
                else
                {
                    if (_inventory.TryGetContainerSlotEnumerator((uid, inventory1), out var enumerator))
                    {
                        while (enumerator.MoveNext(out var slot))
                        {
                            _inventory.TryUnequip(uid, slot.ID, true, true);
                        }
                    }

                    foreach (var held in _hands.EnumerateHeld(uid))
                    {
                        _hands.TryDrop(uid, held);
                    }
                }
            }
            // Goob edit end
        }
        else if (configuration.Inventory == PolymorphInventoryChange.Drop)
        {
            if (_inventory.TryGetContainerSlotEnumerator(uid, out var enumerator))
            {
                while (enumerator.MoveNext(out var slot))
                {
                    _inventory.TryUnequip(uid, slot.ID, true, true);
                }
            }

            foreach (var held in _hands.EnumerateHeld(uid))
            {
                _hands.TryDrop(uid, held);
            }
        }

        if (configuration.TransferName && TryComp(uid, out MetaDataComponent? targetMeta))
        {
            // Goob edit start
            _metaData.SetEntityName(child,
                TryComp(uid, out NameModifierComponent? modifier) ? modifier.BaseName : targetMeta.EntityName);
            // Goob edit end
        }

        if (configuration.TransferHumanoidAppearance)
        {
            _humanoid.CloneAppearance(child, uid);
        }

        if (configuration.ComponentsToTransfer.Count > 0) // Goobstation
        {
            foreach (var data in configuration.ComponentsToTransfer)
            {
                if (!_compFact.TryGetRegistration(data.Component, out var registration))
                    continue;

                var type = registration.Type;

                if (!EntityManager.TryGetComponent(uid, type, out var component))
                    continue;

                var newComp = _compFact.GetComponent(type);

                if (data.Mirror)
                {
                    if (!HasComp(child, type))
                        AddComp(child, newComp);

                    continue;
                }

                if (!data.Override && HasComp(child, type))
                    continue;

                object? temp = (Component) newComp;
                _serialization.CopyTo(component, ref temp, notNullableOverride: true);
                EntityManager.AddComponent(child, (Component) temp!, true);
            }
        }

        _tag.AddTag(uid, SharedBindSoulSystem.IgnoreBindSoulTag); // Goobstation

        if (_mindSystem.TryGetMind(uid, out var mindId, out var mind))
            _mindSystem.TransferTo(mindId, child, mind: mind);

        _tag.RemoveTag(uid, SharedBindSoulSystem.IgnoreBindSoulTag); // Goobstation

        //Ensures a map to banish the entity to
        EnsurePausedMap();
        if (PausedMap != null)
            _transform.SetParent(uid, targetTransformComp, PausedMap.Value);

        // Raise an event to inform anything that wants to know about the entity swap
        var ev = new PolymorphedEvent(uid, child, false);
        RaiseLocalEvent(uid, ref ev);
        RaiseLocalEvent(child, ref ev);

        // visual effect spawn
        if (configuration.EffectProto != null)
            SpawnAttachedTo(configuration.EffectProto, child.ToCoordinates());

        return child;
    }

    /// <summary>
    /// Reverts a polymorphed entity back into its original form
    /// </summary>
    /// <param name="uid">The entityuid of the entity being reverted</param>
    /// <param name="component"></param>
    public EntityUid? Revert(Entity<PolymorphedEntityComponent?> ent)
    {
        var (uid, component) = ent;
        if (!Resolve(ent, ref component))
            return null;

        if (Deleted(uid))
            return null;

        if (component.Parent is not { } parent)
            return null;

        if (Deleted(parent))
            return null;

        var uidXform = Transform(uid);
        var parentXform = Transform(parent);

        // Don't swap back onto a terminating grid
        if (TerminatingOrDeleted(uidXform.ParentUid))
            return null;

        if (component.Configuration.ExitPolymorphSound != null)
            _audio.PlayPvs(component.Configuration.ExitPolymorphSound, uidXform.Coordinates);

        _transform.SetParent(parent, parentXform, uidXform.ParentUid);
        _transform.SetCoordinates(parent, parentXform, uidXform.Coordinates, uidXform.LocalRotation);

        component.Reverted = true;

        if (component.Configuration.TransferDamage &&
            TryComp<DamageableComponent>(parent, out var damageParent) &&
            _mobThreshold.GetScaledDamage(uid, parent, out var damage, out var woundableDamage) &&
            damage != null)
        {
            if (TryComp<BodyComponent>(parent, out var parentBody)
                && parentBody.BodyType == Shared._Shitmed.Body.BodyType.Complex // Too lazy to come up with a new name lmfao
                && _body.TryGetRootPart(parent, out var rootPart, parentBody))
            {
                var woundables = _wound.GetAllWoundableChildrenWithComp<DamageableComponent>(rootPart.Value);
                var count = woundables.Count();
                foreach (var woundable in woundables)
                {
                    var target = _body.GetTargetBodyPart(woundable);

                    if (woundableDamage is not null)
                    {
                        if (woundableDamage.TryGetValue(target, out var wounds))
                            _damageable.SetDamage(woundable, woundable.Comp2, wounds);
                    }
                    else
                    {
                        _damageable.SetDamage(woundable, woundable.Comp2, damage / count);
                    }
                }

            }
            _damageable.SetDamage(parent, damageParent, damage);
        }

        if (component.Configuration.Inventory == PolymorphInventoryChange.Transfer)
        {
            _inventory.TransferEntityInventories(uid, parent);
            foreach (var held in _hands.EnumerateHeld(uid))
            {
                _hands.TryDrop(uid, held);
                _hands.TryPickupAnyHand(parent, held, checkActionBlocker: false);
            }
        }
        else if (component.Configuration.Inventory == PolymorphInventoryChange.Drop)
        {
            if (_inventory.TryGetContainerSlotEnumerator(uid, out var enumerator))
            {
                while (enumerator.MoveNext(out var slot))
                {
                    _inventory.TryUnequip(uid, slot.ID);
                }
            }

            foreach (var held in _hands.EnumerateHeld(uid))
            {
                _hands.TryDrop(uid, held);
            }
        }

        _tag.AddTag(uid, SharedBindSoulSystem.IgnoreBindSoulTag); // Goobstation

        if (_mindSystem.TryGetMind(uid, out var mindId, out var mind))
            _mindSystem.TransferTo(mindId, parent, mind: mind);

        _tag.RemoveTag(uid, SharedBindSoulSystem.IgnoreBindSoulTag); // Goobstation

        if (TryComp<PolymorphableComponent>(parent, out var polymorphableComponent))
            polymorphableComponent.LastPolymorphEnd = _gameTiming.CurTime;

        // if an item polymorph was picked up, put it back down after reverting
        _transform.AttachToGridOrMap(parent, parentXform);

        // Raise an event to inform anything that wants to know about the entity swap
        var ev = new PolymorphedEvent(uid, parent, true);
        RaiseLocalEvent(uid, ref ev);
        RaiseLocalEvent(parent, ref ev);

        // visual effect spawn
        if (component.Configuration.EffectProto != null)
            SpawnAttachedTo(component.Configuration.EffectProto, parent.ToCoordinates());

        var popup = Loc.GetString("polymorph-revert-popup-generic",
                    ("parent", Identity.Entity(uid, EntityManager)),
                    ("child", Identity.Entity(parent, EntityManager)));

        if (component.Configuration.ExitPolymorphPopup != null)
            popup = Loc.GetString(component.Configuration.ExitPolymorphPopup,
                ("parent", Identity.Entity(uid, EntityManager)),
                ("child", Identity.Entity(parent, EntityManager)));

        if (component.Configuration.ShowPopup)
            _popup.PopupEntity(popup, parent);
        QueueDel(uid);

        return parent;
    }

    /// <summary>
    /// Creates a sidebar action for an entity to be able to polymorph at will
    /// </summary>
    /// <param name="id">The string of the id of the polymorph action</param>
    /// <param name="target">The entity that will be gaining the action</param>
    public void CreatePolymorphAction(ProtoId<PolymorphPrototype> id, Entity<PolymorphableComponent> target)
    {
        target.Comp.PolymorphActions ??= new();
        if (target.Comp.PolymorphActions.ContainsKey(id))
            return;

        if (!_proto.Resolve(id, out var polyProto))
            return;

        // Goob edit start
        if (polyProto.Configuration.Entity == null)
            return;

        var entProto = _proto.Index(polyProto.Configuration.Entity.Value);
        // Goob edit end

        EntityUid? actionId = default!;
        if (!_actions.AddAction(target, ref actionId, RevertPolymorphId, target))
            return;

        target.Comp.PolymorphActions.Add(id, actionId.Value);

        var metaDataCache = MetaData(actionId.Value);
        _metaData.SetEntityName(actionId.Value, Loc.GetString("polymorph-self-action-name", ("target", entProto.Name)), metaDataCache);
        _metaData.SetEntityDescription(actionId.Value, Loc.GetString("polymorph-self-action-description", ("target", entProto.Name)), metaDataCache);

        if (_actions.GetAction(actionId) is not {} action)
            return;

        _actions.SetIcon((action, action.Comp), new SpriteSpecifier.EntityPrototype(polyProto.Configuration.Entity));
        _actions.SetEvent(action, new PolymorphActionEvent(id));
    }

    public void RemovePolymorphAction(ProtoId<PolymorphPrototype> id, Entity<PolymorphableComponent> target)
    {
        if (target.Comp.PolymorphActions is not {} actions)
            return;

        if (actions.TryGetValue(id, out var action))
            _actions.RemoveAction(target.Owner, action);
    }

    // goob edit
    // it makes more sense for it to be here than anywhere.
    // if anywhere it should be embedded in the engine but we can't afford that :P
    public T? CopyPolymorphComponent<T>(EntityUid old, EntityUid @new, bool transfer = true) where T : Component
        => CopyPolymorphComponent(old, @new, typeof(T), transfer) as T;

    // don't use transfer if you have component references like EE languages
    // ideally you shouldn't use comp references at all
    public IComponent? CopyPolymorphComponent(EntityUid old, EntityUid @new, string componentRegistration, bool transfer = true)
    {
        if (!_compFact.TryGetRegistration(componentRegistration, out var reg))
            return null;

        return CopyPolymorphComponent(old, @new, reg.Type, transfer);
    }

    public IComponent? CopyPolymorphComponent(EntityUid old, EntityUid @new, Type compType, bool transfer = true)
    {
        if (old == @new)
            return null;

        if (!EntityManager.TryGetComponent(old, compType, out var comp))
            return null;

        if (transfer)
        {
            var newComp = (Component) _compFact.GetComponent(compType);
            var temp = (object) newComp;
            _serialization.CopyTo(comp, ref temp, notNullableOverride: true);
            EntityManager.AddComponent(@new, (Component) temp!, true);
            return temp as IComponent;
        }

        var copy = _serialization.CreateCopy(comp, notNullableOverride: true);
        AddComp(@new, copy, true);
        return copy;
    }
    // goob edit end
}

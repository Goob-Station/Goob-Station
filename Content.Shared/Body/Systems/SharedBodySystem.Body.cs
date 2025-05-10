// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Doru991 <75124791+Doru991@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Psychpsyo <60073468+Psychpsyo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ShadowCommander <shadowjjt@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Prototypes;
using Content.Shared.DragDrop;
using Content.Shared.Gibbing.Components;
using Content.Shared.Gibbing.Events;
using Content.Shared.Gibbing.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Utility;

// Shitmed Change
using Content.Shared._Shitmed.Body.Events;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared._Shitmed.Humanoid.Events;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Mobs.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory.Events;
using Content.Shared.Pulling.Events;
using Content.Shared.Standing;
using Robust.Shared.Network;
using Content.Shared.Rejuvenate;
using Content.Shared.Popups;
using Robust.Shared.Timing;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    /*
     * tl;dr of how bobby works
     * - BodyComponent uses a BodyPrototype as a template.
     * - On MapInit we spawn the root entity in the prototype and spawn all connections outwards from here
     * - Each "connection" is a body part (e.g. arm, hand, etc.) and each part can also contain organs.
     */

    [Dependency] private readonly GibbingSystem _gibbingSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    // Shitmed Change Start
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly WoundSystem _woundSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    // Shitmed Change End

    private const float GibletLaunchImpulse = 8;
    private const float GibletLaunchImpulseVariance = 3;

    private void InitializeBody()
    {
        // Body here to handle root body parts.
        SubscribeLocalEvent<BodyComponent, EntInsertedIntoContainerMessage>(OnBodyInserted);
        SubscribeLocalEvent<BodyComponent, EntRemovedFromContainerMessage>(OnBodyRemoved);

        SubscribeLocalEvent<BodyComponent, ComponentInit>(OnBodyInit);
        SubscribeLocalEvent<BodyComponent, MapInitEvent>(OnBodyMapInit);
        SubscribeLocalEvent<BodyComponent, CanDragEvent>(OnBodyCanDrag);
        SubscribeLocalEvent<BodyComponent, StandAttemptEvent>(OnStandAttempt); // Shitmed Change
        SubscribeLocalEvent<BodyComponent, ProfileLoadFinishedEvent>(OnProfileLoadFinished); // Shitmed change
        SubscribeLocalEvent<BodyComponent, IsEquippingAttemptEvent>(OnBeingEquippedAttempt); // Shitmed Change
        SubscribeLocalEvent<BodyComponent, AttemptStopPullingEvent>(OnAttemptStopPulling); // Goobstation

        // Shitmed Change: to prevent people from falling immediately as rejuvenated
        SubscribeLocalEvent<BodyComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private void OnAttemptStopPulling(Entity<BodyComponent> ent, ref AttemptStopPullingEvent args) // Goobstation
    {
        if (args.User == null || !Exists(args.User.Value))
            return;

        if (args.User.Value != ent.Owner)
            return;

        if (ent.Comp.LegEntities.Count > 0 || ent.Comp.RequiredLegs == 0)
            return;

        args.Cancelled = true;
    }

    private void OnBodyInserted(Entity<BodyComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        // Root body part?
        var slotId = args.Container.ID;

        if (slotId != BodyRootContainerId)
            return;

        var insertedUid = args.Entity;

        if (TryComp(insertedUid, out BodyPartComponent? part))
        {
            AddPart((ent, ent), (insertedUid, part), slotId);
            RecursiveBodyUpdate((insertedUid, part), ent);
        }

        if (TryComp(insertedUid, out OrganComponent? organ))
        {
            AddOrgan((insertedUid, organ), ent, ent);
        }
    }

    private void OnBodyRemoved(Entity<BodyComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        // Root body part?
        var slotId = args.Container.ID;

        if (slotId != BodyRootContainerId)
            return;

        var removedUid = args.Entity;
        DebugTools.Assert(!TryComp(removedUid, out BodyPartComponent? b) || b.Body == ent);
        DebugTools.Assert(!TryComp(removedUid, out OrganComponent? o) || o.Body == ent);

        if (TryComp(removedUid, out BodyPartComponent? part))
        {
            RemovePart((ent, ent), (removedUid, part), slotId);
            RecursiveBodyUpdate((removedUid, part), null);
        }

        if (TryComp(removedUid, out OrganComponent? organ))
            RemoveOrgan((removedUid, organ), ent);
    }

    private void OnBodyInit(Entity<BodyComponent> ent, ref ComponentInit args)
    {
        // Setup the initial container.
        ent.Comp.RootContainer = Containers.EnsureContainer<ContainerSlot>(ent, BodyRootContainerId);
    }

    private void OnBodyMapInit(Entity<BodyComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.Prototype is null)
            return;

        // One-time setup
        // Obviously can't run in Init to avoid double-spawns on save / load.
        var prototype = Prototypes.Index(ent.Comp.Prototype.Value);
        MapInitBody(ent, prototype);
        EnsureComp<SurgeryTargetComponent>(ent); // Shitmed change
    }

    private void MapInitBody(EntityUid bodyEntity, BodyPrototype prototype)
    {
        var protoRoot = prototype.Slots[prototype.Root];
        if (protoRoot.Part is null)
            return;

        // This should already handle adding the entity to the root.
        var rootPartUid = SpawnInContainerOrDrop(protoRoot.Part, bodyEntity, BodyRootContainerId);
        var rootPart = Comp<BodyPartComponent>(rootPartUid);
        rootPart.Body = bodyEntity;
        Dirty(rootPartUid, rootPart);

        // Setup the rest of the body entities.
        SetupOrgans((rootPartUid, rootPart), protoRoot.Organs);
        MapInitParts(rootPartUid, rootPart, prototype); // Shitmed Change
    }

    private void OnBodyCanDrag(Entity<BodyComponent> ent, ref CanDragEvent args)
    {
        args.Handled = true;
    }

    /// <summary>
    /// Sets up all of the relevant body parts for a particular body entity and root part.
    /// </summary>
    private void MapInitParts(EntityUid rootPartId, BodyPartComponent rootPart, BodyPrototype prototype) // Shitmed Change
    {
        // Start at the root part and traverse the body graph, setting up parts as we go.
        // Basic BFS pathfind.
        var rootSlot = prototype.Root;
        var frontier = new Queue<string>();
        frontier.Enqueue(rootSlot);

        // Child -> Parent connection.
        var cameFrom = new Dictionary<string, string>();
        cameFrom[rootSlot] = rootSlot;
        // Maps slot to its relevant entity.
        var cameFromEntities = new Dictionary<string, EntityUid>();
        cameFromEntities[rootSlot] = rootPartId;

        while (frontier.TryDequeue(out var currentSlotId))
        {
            var currentSlot = prototype.Slots[currentSlotId];

            foreach (var connection in currentSlot.Connections)
            {
                // Already been handled
                if (!cameFrom.TryAdd(connection, currentSlotId))
                    continue;

                // Setup part
                var connectionSlot = prototype.Slots[connection];
                var parentEntity = cameFromEntities[currentSlotId];
                var parentPartComponent = Comp<BodyPartComponent>(parentEntity);

                // Spawn the entity on the target
                // then get the body part type, create the slot, and finally
                // we can insert it into the container.
                var childPart = Spawn(connectionSlot.Part, new EntityCoordinates(parentEntity, Vector2.Zero));
                cameFromEntities[connection] = childPart;

                var childPartComponent = Comp<BodyPartComponent>(childPart);
                TryCreatePartSlot(parentEntity, connection, childPartComponent.PartType, childPartComponent.Symmetry, out var partSlot, parentPartComponent);
                // Shitmed Change Start
                childPartComponent.ParentSlot = partSlot;
                Dirty(childPart, childPartComponent);
                // Shitmed Change End
                var cont = Containers.GetContainer(parentEntity, GetPartSlotContainerId(connection));

                if (partSlot is null || !Containers.Insert(childPart, cont))
                {
                    Log.Error($"Could not create slot for connection {connection} in body {prototype.ID}");
                    QueueDel(childPart);
                    continue;
                }

                // Add organs
                SetupOrgans((childPart, childPartComponent), connectionSlot.Organs);

                // Enqueue it so we can also get its neighbors.
                frontier.Enqueue(connection);
            }
        }
    }

    private void SetupOrgans(Entity<BodyPartComponent> ent, Dictionary<string, string> organs)
    {
        foreach (var (organSlotId, organProto) in organs)
        {
            TryCreateOrganSlot(ent, organSlotId, out var slot); // Shitmed Change
            SpawnInContainerOrDrop(organProto, ent, GetOrganContainerId(organSlotId));

            if (slot is null)
            {
                Log.Error($"Could not create organ for slot {organSlotId} in {ToPrettyString(ent)}");
            }
        }
    }

    /// <summary>
    /// Gets all body containers on this entity including the root one.
    /// </summary>
    public IEnumerable<BaseContainer> GetBodyContainers(
        EntityUid id,
        BodyComponent? body = null,
        BodyPartComponent? rootPart = null)
    {
        if (!Resolve(id, ref body, logMissing: false)
            || body.RootContainer.ContainedEntity is null
            || !Resolve(body.RootContainer.ContainedEntity.Value, ref rootPart))
        {
            yield break;
        }

        yield return body.RootContainer;

        foreach (var childContainer in GetPartContainers(body.RootContainer.ContainedEntity.Value, rootPart))
        {
            yield return childContainer;
        }
    }

    /// <summary>
    /// Gets all child body parts of this entity, including the root entity.
    /// </summary>
    public IEnumerable<(EntityUid Id, BodyPartComponent Component)> GetBodyChildren(
        EntityUid? id,
        BodyComponent? body = null,
        BodyPartComponent? rootPart = null)
    {
        if (id is null
            || !Resolve(id.Value, ref body, logMissing: false)
            || body is null // Shitmed Change
            || body.RootContainer == null // Shitmed Change
            || body.RootContainer.ContainedEntity is null
            || !Resolve(body.RootContainer.ContainedEntity.Value, ref rootPart))
        {
            yield break;
        }

        foreach (var child in GetBodyPartChildren(body.RootContainer.ContainedEntity.Value, rootPart))
        {
            yield return child;
        }
    }

    public IEnumerable<(EntityUid Id, OrganComponent Component)> GetBodyOrgans(
        EntityUid? bodyId,
        BodyComponent? body = null)
    {
        if (bodyId is null || !Resolve(bodyId.Value, ref body, logMissing: false))
            yield break;

        foreach (var part in GetBodyChildren(bodyId, body))
        {
            foreach (var organ in GetPartOrgans(part.Id, part.Component))
            {
                yield return organ;
            }
        }
    }

    /// <summary>
    /// Returns all body part slots for this entity.
    /// </summary>
    /// <param name="bodyId"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public IEnumerable<BodyPartSlot> GetBodyAllSlots(
        EntityUid bodyId,
        BodyComponent? body = null)
    {
        if (!Resolve(bodyId, ref body, logMissing: false)
            || body.RootContainer.ContainedEntity is null)
        {
            yield break;
        }

        foreach (var slot in GetAllBodyPartSlots(body.RootContainer.ContainedEntity.Value))
        {
            yield return slot;
        }
    }

    public virtual HashSet<EntityUid> GibBody(
        EntityUid bodyId,
        bool gibOrgans = false,
        BodyComponent? body = null,
        bool launchGibs = true,
        Vector2? splatDirection = null,
        float splatModifier = 1,
        Angle splatCone = default,
        SoundSpecifier? gibSoundOverride = null,
        // Shitmed Change
        GibType gib = GibType.Gib,
        GibContentsOption contents = GibContentsOption.Drop,
        List<string>? allowedContainers = null,
        List<string>? excludedContainers = null)
    {
        var gibs = new HashSet<EntityUid>();

        if (!Resolve(bodyId, ref body, logMissing: false))
            return gibs;

        if (TryGetRootPart(bodyId, out var root) && TryComp(root.Value.Owner, out GibbableComponent? gibbable)) // ShitMed - TryGet
            gibSoundOverride ??= gibbable.GibSound;


        var parts = GetBodyChildren(bodyId, body).ToArray();
        gibs.EnsureCapacity(parts.Length);
        foreach (var part in parts)
        {

            _gibbingSystem.TryGibEntityWithRef(bodyId, part.Id, gib, contents, ref gibs, playAudio: false,
                launchGibs: true, launchDirection: splatDirection, launchImpulse: GibletLaunchImpulse * splatModifier,
                launchImpulseVariance: GibletLaunchImpulseVariance, launchCone: splatCone,
                allowedContainers: allowedContainers, excludedContainers: excludedContainers);

            if (!gibOrgans)
                continue;

            foreach (var organ in GetPartOrgans(part.Id, part.Component))
            {
                _gibbingSystem.TryGibEntityWithRef(bodyId, organ.Id, GibType.Drop, GibContentsOption.Skip,
                    ref gibs, playAudio: false, launchImpulse: GibletLaunchImpulse * splatModifier,
                    launchImpulseVariance: GibletLaunchImpulseVariance, launchCone: splatCone,
                    allowedContainers: allowedContainers, excludedContainers: excludedContainers);
            }
        }

        var bodyTransform = Transform(bodyId);
        if (TryComp<InventoryComponent>(bodyId, out var inventory))
        {
            foreach (var item in _inventory.GetHandOrInventoryEntities(bodyId))
            {
                SharedTransform.DropNextTo(item, (bodyId, bodyTransform));
                gibs.Add(item);
            }
        }
        _audioSystem.PlayPredicted(gibSoundOverride, bodyTransform.Coordinates, null);
        return gibs;
    }

    // Shitmed Change Start

    public virtual HashSet<EntityUid> GibPart(
        EntityUid partId,
        BodyPartComponent? part = null,
        bool launchGibs = true,
        Vector2? splatDirection = null,
        float splatModifier = 1,
        Angle splatCone = default,
        SoundSpecifier? gibSoundOverride = null)
    {
        var gibs = new HashSet<EntityUid>();

        if (!Resolve(partId, ref part, logMissing: false))
            return gibs;

        if (part.Body is { } bodyEnt)
        {
            if (IsPartRoot(bodyEnt, partId, part: part))
                return gibs;

            DropSlotContents((partId, part));
            foreach (var organ in GetPartOrgans(partId, part))
            {
                _gibbingSystem.TryGibEntityWithRef(bodyEnt, organ.Id, GibType.Drop, GibContentsOption.Skip,
                    ref gibs, playAudio: false, launchImpulse: GibletLaunchImpulse * splatModifier,
                    launchImpulseVariance: GibletLaunchImpulseVariance, launchCone: splatCone);
            }
        }

        _gibbingSystem.TryGibEntityWithRef(partId, partId, GibType.Gib, GibContentsOption.Drop, ref gibs,
                playAudio: true, launchGibs: true, launchDirection: splatDirection, launchImpulse: GibletLaunchImpulse * splatModifier,
                launchImpulseVariance: GibletLaunchImpulseVariance, launchCone: splatCone);


        if (HasComp<InventoryComponent>(partId))
        {
            foreach (var item in _inventory.GetHandOrInventoryEntities(partId))
            {
                SharedTransform.AttachToGridOrMap(item);
                gibs.Add(item);
            }
        }
        _audioSystem.PlayPredicted(gibSoundOverride, Transform(partId).Coordinates, null);
        return gibs;
    }

    public virtual bool BurnPart(EntityUid partId,
        BodyPartComponent? part = null)
    {
        if (!Resolve(partId, ref part, logMissing: false))
            return false;

        if (part.Body is { } bodyEnt)
        {
            if (IsPartRoot(bodyEnt, partId, part: part))
                return false;

            DropSlotContents((partId, part));
            QueueDel(partId);
            return true;
        }

        return false;
    }

    private void OnProfileLoadFinished(EntityUid uid, BodyComponent component, ProfileLoadFinishedEvent args)
    {
        if (!HasComp<HumanoidAppearanceComponent>(uid)
            || TerminatingOrDeleted(uid)
            || !Initialized(uid)) // We do this last one for urists on test envs.
            return;

        foreach (var part in GetBodyChildren(uid, component))
            EnsureComp<BodyPartAppearanceComponent>(part.Id);
    }

    private void OnStandAttempt(Entity<BodyComponent> ent, ref StandAttemptEvent args)
    {
        if (ent.Comp.LegEntities.Count < ent.Comp.RequiredLegs)
            args.Cancel();
    }

    private void OnBeingEquippedAttempt(Entity<BodyComponent> ent, ref IsEquippingAttemptEvent args)
    {
        if (!TryComp(args.EquipTarget, out BodyComponent? targetBody)
            || targetBody.Prototype == null
            || HasComp<BorgChassisComponent>(args.EquipTarget))
            return;

        if (TryGetPartFromSlotContainer(args.Slot, out var bodyPart)
            && bodyPart is not null)
        {
            var bodyPartString = bodyPart.Value.ToString().ToLower();
            var prototype = Prototypes.Index(targetBody.Prototype.Value);
            var hasPartConnection = prototype.Slots.Values.Any(slot =>
                slot.Connections.Contains(bodyPartString));

            if (hasPartConnection
                && !GetBodyChildrenOfType(args.EquipTarget, bodyPart.Value).Any())
            {
                _popup.PopupClient(Loc.GetString("equip-part-missing-error",
                    ("target", args.EquipTarget), ("part", bodyPartString)), args.Equipee, args.Equipee);
                args.Cancel();
            }
        }
    }

    private void OnRejuvenate(EntityUid ent, BodyComponent body, ref RejuvenateEvent args)
    {
        if (body.Prototype == null)
            return;

        var prototype = Prototypes.Index(body.Prototype.Value);

        if (!TryGetRootPart(ent, out var rootPart))
            return;

        var rootSlot = prototype.Root;
        foreach (var organ in prototype.Slots[rootSlot].Organs)
        {
            if (!Containers.TryGetContainer(rootPart.Value.Owner, GetOrganContainerId(organ.Key), out var organContainer))
                continue;

            var organEnt = organContainer.ContainedEntities.FirstOrNull();
            if (organEnt != null)
            {
                foreach (var modifier in Comp<OrganComponent>(organEnt.Value).IntegrityModifiers)
                {
                    _trauma.TryRemoveOrganDamageModifier(organEnt.Value, modifier.Key.Item2, modifier.Key.Item1);
                }
            }
            else
            {
                SpawnInContainerOrDrop(organ.Value, rootPart.Value.Owner, GetOrganContainerId(organ.Key));
            }
        }

        Dirty(rootPart.Value.Owner, rootPart.Value.Comp);

        var frontier = new Queue<string>();
        frontier.Enqueue(rootSlot);

        var cameFrom = new Dictionary<string, string>();
        cameFrom[rootSlot] = rootSlot;

        var cameFromEntities = new Dictionary<string, EntityUid>();
        cameFromEntities[rootSlot] = rootPart.Value.Owner;

        while (frontier.TryDequeue(out var currentSlotId))
        {
            var currentSlot = prototype.Slots[currentSlotId];

            foreach (var connection in currentSlot.Connections)
            {
                if (!cameFrom.TryAdd(connection, currentSlotId))
                    continue;

                var connectionSlot = prototype.Slots[connection];
                var parentEntity = cameFromEntities[currentSlotId];
                var parentPartComponent = Comp<BodyPartComponent>(parentEntity);

                if (Containers.TryGetContainer(parentEntity, GetPartSlotContainerId(connection), out var container))
                {
                    if (container.ContainedEntities.Count > 0)
                    {
                        var containedEnt = container.ContainedEntities[0];
                        var containedPartComp = Comp<BodyPartComponent>(containedEnt);
                        cameFromEntities[connection] = containedEnt;

                        foreach (var organ in connectionSlot.Organs)
                        {
                            if (Containers.TryGetContainer(containedEnt, GetOrganContainerId(organ.Key), out var organContainer))
                            {
                                var organEnt = organContainer.ContainedEntities.FirstOrNull();
                                if (organEnt != null)
                                {
                                    foreach (var modifier in Comp<OrganComponent>(organEnt.Value).IntegrityModifiers)
                                    {
                                        _trauma.TryRemoveOrganDamageModifier(organEnt.Value, modifier.Key.Item2, modifier.Key.Item1);
                                    }
                                }
                                else
                                {
                                    SpawnInContainerOrDrop(organ.Value, containedEnt, GetOrganContainerId(organ.Key));
                                }
                            }
                            else
                            {
                                var slot = CreateOrganSlot((containedEnt, containedPartComp), organ.Key);
                                SpawnInContainerOrDrop(organ.Value, containedEnt, GetOrganContainerId(organ.Key));

                                if (slot is null)
                                {
                                    Log.Error($"Could not create organ for slot {organ.Key} in {ToPrettyString(ent)}");
                                }
                            }
                        }
                    }
                    else
                    {
                        var childPart = Spawn(connectionSlot.Part, new EntityCoordinates(parentEntity, Vector2.Zero));
                        cameFromEntities[connection] = childPart;

                        var childPartComponent = Comp<BodyPartComponent>(childPart);

                        var partSlot = new BodyPartSlot(connection, childPartComponent.PartType, childPartComponent.Symmetry);
                        childPartComponent.ParentSlot = partSlot;
                        parentPartComponent.Children.TryAdd(connection, partSlot);

                        Dirty(parentEntity, parentPartComponent);
                        Dirty(childPart, childPartComponent);

                        Containers.Insert(childPart, container);

                        SetupOrgans((childPart, childPartComponent), connectionSlot.Organs);
                    }
                }
                else
                {
                    var childPart = Spawn(connectionSlot.Part, new EntityCoordinates(parentEntity, Vector2.Zero));
                    cameFromEntities[connection] = childPart;

                    var childPartComponent = Comp<BodyPartComponent>(childPart);

                    var partSlot = CreatePartSlot(parentEntity, connection, childPartComponent.PartType, childPartComponent.Symmetry, parentPartComponent);
                    childPartComponent.ParentSlot = partSlot;

                    Dirty(parentEntity, parentPartComponent);
                    Dirty(childPart, childPartComponent);

                    if (partSlot is null)
                    {
                        Log.Error($"Could not create slot for connection {connection} in body {prototype.ID}");
                        QueueDel(childPart);
                        continue;
                    }

                    container = Containers.GetContainer(parentEntity, GetPartSlotContainerId(connection));
                    Containers.Insert(childPart, container);

                    SetupOrgans((childPart, childPartComponent), connectionSlot.Organs);
                }

                frontier.Enqueue(connection);
            }
        }


        if (_trauma.TryGetBodyTraumas(ent, out var traumas, bodyComp: body))
            foreach (var trauma in traumas)
                _trauma.RemoveTrauma(trauma);

        foreach (var bodyPart in GetBodyChildren(ent, body))
        {
            if (!TryComp<WoundableComponent>(bodyPart.Id, out var woundable))
                continue;

            var bone = woundable.Bone.ContainedEntities.FirstOrNull();
            if (TryComp<BoneComponent>(bone, out var boneComp))
                _trauma.SetBoneIntegrity(bone.Value, boneComp.IntegrityCap, boneComp);

            _woundSystem.TryHaltAllBleeding(bodyPart.Id, woundable);
            _woundSystem.ForceHealWoundsOnWoundable(bodyPart.Id, out _);
        }
    }
    // Shitmed Change End
}

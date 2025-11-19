// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared._Funkystation.Factory.Components;
using Content.Shared.Body.Systems;
using Robust.Shared.Containers;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Map;
using Content.Server.Body.Components;
using Content.Server._Funkystation.Factory.Components;
using Content.Server.Silicons.Laws;
using Content.Server.Mind;
using Content.Server._Funkystation.Robotics.Systems;
using Content.Shared._Funkystation.Materials;

namespace Content.Server._Funkystation.Factory.Systems;

/// <summary>
/// System that handles cyborg factory functionality - converting entities with minds into cyborgs
/// </summary>
public sealed class CyborgFactorySystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SiliconLawSystem _laws = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly CyborgLawReceiverSystem _cyborgLawReceiver = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

    // Constants for entity prototypes and slot names
    private const string MmiPrototype = "MMI";
    private const string CyborgPrototype = "PlayerBorgBatteryNoMind";
    private const string BrainSlotId = "brain_slot";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoboticsFactoryGridComponent, MaterialReclaimerProcessEntityEvent>(OnEntityProcessed);
    }

    /// <summary>
    /// Handles entities being processed by the robotics factory grid
    /// </summary>
    private void OnEntityProcessed(EntityUid factoryUid, RoboticsFactoryGridComponent component, MaterialReclaimerProcessEntityEvent args)
    {
        var entity = args.Entity;

        // Don't process crew if unanchored
        if (!Transform(factoryUid).Anchored)
        {
            args.Handled = true;
            return;
        }

        // Capture prior name (if any) before gibbing
        string? priorName = null;

        var meta = MetaData(entity);

        priorName = meta.EntityName;

        // Validate the entity for conversion
        if (!ValidateEntityForConversion(entity, out var mindId))
            return;

        var spawnCoords = Transform(factoryUid).Coordinates;

        // Process gibbing and extract brain
        if (!ProcessEntityGibbing(entity, out var brainUid))
        {
            args.Handled = true;
            return;
        }

        // Create cyborg from brain
        if (!CreateCyborgFromBrain(brainUid, spawnCoords, out var cyborg, out var mmi))
        {
            args.Handled = true;
            return;
        }

        // Restore prior name (if available)
        if (!string.IsNullOrWhiteSpace(priorName))
            _meta.SetEntityName(cyborg, priorName);

        // Configure cyborg for Malf AI control
        ConfigureCyborgForMalfAI(factoryUid, cyborg);

        // Cancel default recycling for the original entity we already handled via gib
        args.Handled = true;
    }

    /// <summary>
    /// Validates if an entity can be converted to a cyborg
    /// </summary>
    private bool ValidateEntityForConversion(EntityUid entity, out EntityUid mindId)
    {
        mindId = EntityUid.Invalid;

        // Check if entity has a mind
        if (!TryComp<MindContainerComponent>(entity, out var mindContainer) || !mindContainer.HasMind)
            return false;

        // Check if entity is already a cyborg (has BorgChassis component)
        if (HasComp<BorgChassisComponent>(entity))
            return false;

        // Get the mind
        if (!_mind.TryGetMind(entity, out mindId, out var mind))
            return false;

        // Check if mind has a user (player-controlled)
        if (mind.UserId == null)
            return false;

        return true;
    }

    /// <summary>
    /// Processes entity gibbing and extracts the brain organ
    /// </summary>
    private bool ProcessEntityGibbing(EntityUid entity, out EntityUid brainUid)
    {
        brainUid = EntityUid.Invalid;

        // Gib the entity and obtain its brain organ entity
        var gibbed = _body.GibBody(entity, gibOrgans: true);
        if (gibbed.Count == 0)
        {
            return false;
        }

        foreach (var ent in gibbed)
        {
            if (HasComp<BrainComponent>(ent))
            {
                brainUid = ent;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a cyborg from a brain by spawning an MMI and inserting it into a cyborg chassis
    /// </summary>
    private bool CreateCyborgFromBrain(EntityUid brainUid, EntityCoordinates spawnCoords, out EntityUid cyborg, out EntityUid mmi)
    {
        cyborg = EntityUid.Invalid;
        mmi = EntityUid.Invalid;

        // Spawn an MMI and insert the brain into it
        mmi = EntityManager.SpawnEntity(MmiPrototype, spawnCoords);
        if (!_itemSlots.TryInsert(mmi, BrainSlotId, brainUid, user: null))
        {
            QueueDel(mmi);
            return false;
        }

        // Spawn a cyborg chassis and insert the MMI into the borg's brain container
        cyborg = EntityManager.SpawnEntity(CyborgPrototype, spawnCoords);
        if (!TryComp<BorgChassisComponent>(cyborg, out var chassis))
        {
            QueueDel(cyborg);
            QueueDel(mmi);
            return false;
        }

        _containers.Insert(mmi, chassis.BrainContainer);
        return true;
    }

    /// <summary>
    /// Configures a cyborg for Malf AI control by assigning ownership and imposing Law 0
    /// </summary>
    private void ConfigureCyborgForMalfAI(EntityUid factoryUid, EntityUid cyborg)
    {
        // Get the Malf AI controller for ownership tracking
        EntityUid? malfAi = null;
        if (TryComp<MalfFactoryOwnerComponent>(factoryUid, out var owner) && owner.Controller != null)
        {
            malfAi = owner.Controller;
        }

        // - Ownership tracking for malf AI cyborg menu
        _cyborgLawReceiver.ImposeLawZero(cyborg, malfAi);
    }
}

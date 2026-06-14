// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Body.Components;
using Content.Server._Funkystation.MalfAI.Borgs;
using Content.Shared.Body.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Factory;
using Content.Shared._Funkystation.Materials;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Server._Funkystation.MalfAI.Factory;

/// <summary>
/// Converts crew processed by the robotics factory into cyborgs subservient to the Malf AI.
/// Law subversion is handled by the AI law sync system once the borg is created.
/// </summary>
public sealed class CyborgFactorySystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly MalfAiBorgsUiSystem _borgsUi = default!;

    private const string MmiPrototype = "MMI";
    private const string CyborgPrototype = "MalfAiFactoryBorgChassis";
    private const string BrainSlotId = "brain_slot";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoboticsFactoryGridComponent, MaterialReclaimerProcessEntityEvent>(OnEntityProcessed);
    }

    private void OnEntityProcessed(Entity<RoboticsFactoryGridComponent> factory, ref MaterialReclaimerProcessEntityEvent args)
    {
        var entity = args.Entity;

        if (!Transform(factory.Owner).Anchored)
        {
            args.Handled = true;
            return;
        }

        var priorName = MetaData(entity).EntityName;

        if (!ValidateEntityForConversion(entity))
            return;

        var spawnCoords = Transform(factory.Owner).Coordinates;

        if (!TryExtractBrain(entity, out var brainUid))
        {
            args.Handled = true;
            return;
        }

        if (!TryCreateCyborgFromBrain(brainUid, spawnCoords, out var cyborg))
        {
            args.Handled = true;
            return;
        }

        if (!string.IsNullOrWhiteSpace(priorName))
            _meta.SetEntityName(cyborg, priorName);

        // Auto-assign ownership to the AI that built this factory.
        if (TryComp<MalfFactoryOwnerComponent>(factory.Owner, out var owner) &&
            owner.Controller != null &&
            HasComp<MalfAiMarkerComponent>(owner.Controller.Value))
        {
            var controlled = EnsureComp<MalfAiControlledComponent>(cyborg);
            controlled.Controller = owner.Controller;
            Dirty(cyborg, controlled);

            _borgsUi.RefreshBorgsUi(owner.Controller.Value);
        }

        args.Handled = true;
    }

    private bool ValidateEntityForConversion(EntityUid entity)
    {
        if (!TryComp<MindContainerComponent>(entity, out var mindContainer) || !mindContainer.HasMind)
            return false;

        if (HasComp<BorgChassisComponent>(entity))
            return false;

        if (!_mind.TryGetMind(entity, out _, out var mind) || mind.UserId == null)
            return false;

        return true;
    }

    private bool TryExtractBrain(EntityUid entity, out EntityUid brainUid)
    {
        brainUid = EntityUid.Invalid;

        var gibbed = _body.GibBody(entity, gibOrgans: true);
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

    private bool TryCreateCyborgFromBrain(EntityUid brainUid, EntityCoordinates spawnCoords, out EntityUid cyborg)
    {
        cyborg = EntityUid.Invalid;

        var mmi = Spawn(MmiPrototype, spawnCoords);
        if (!_itemSlots.TryInsert(mmi, BrainSlotId, brainUid, user: null))
        {
            QueueDel(mmi);
            return false;
        }

        cyborg = Spawn(CyborgPrototype, spawnCoords);
        if (!TryComp<BorgChassisComponent>(cyborg, out var chassis))
        {
            QueueDel(cyborg);
            QueueDel(mmi);
            return false;
        }

        _containers.Insert(mmi, chassis.BrainContainer);
        return true;
    }
}

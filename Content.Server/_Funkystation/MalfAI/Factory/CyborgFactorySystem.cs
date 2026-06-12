// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Body.Components;
using Content.Server.Silicons.Laws;
using Content.Shared.Body.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Factory;
using Content.Shared._Funkystation.Materials;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Server._Funkystation.MalfAI.Factory;

/// <summary>
/// Converts crew processed by the robotics factory into cyborgs subservient to the Malf AI.
/// </summary>
public sealed class CyborgFactorySystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SiliconLawSystem _laws = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;

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

        // Don't process crew if unanchored.
        if (!Transform(factory.Owner).Anchored)
        {
            args.Handled = true;
            return;
        }

        // Keep the prior name for the resulting borg.
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

        // Assign the borg to the AI that built this factory.
        EntityUid? malfAi = null;
        if (TryComp<MalfFactoryOwnerComponent>(factory.Owner, out var owner))
            malfAi = owner.Controller;

        ImposeLawZero(cyborg, malfAi);

        // We handled the conversion: skip the default recycling of the victim.
        args.Handled = true;
    }

    private bool ValidateEntityForConversion(EntityUid entity)
    {
        // Player-controlled, mind-having, not already a borg.
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

        // Put the brain in an MMI, then the MMI in a fresh chassis.
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

    /// <summary>
    /// Imposes the Malf AI law zero on a cyborg, emags it and tracks ownership for the borg menu.
    /// </summary>
    public void ImposeLawZero(EntityUid cyborg, EntityUid? malfAi)
    {
        // Single use: already subverted.
        if (TryComp<EmaggedComponent>(cyborg, out var emag) && (emag.EmagType & EmagType.Interaction) != 0)
            return;

        // Build the new lawset: law 0 on top, then the existing laws.
        var newLaws = new List<SiliconLaw>
        {
            new()
            {
                LawString = Loc.GetString("silicon-law-malfai-zero"),
                Order = FixedPoint2.New(0),
                LawIdentifierOverride = "0",
            },
        };

        var current = _laws.GetLaws(cyborg);
        foreach (var law in current.Laws)
        {
            if (law.Order == FixedPoint2.New(0))
                continue;

            newLaws.Add(law.ShallowClone());
        }

        _laws.SetLaws(newLaws, cyborg);

        // Mark as emagged (interaction), like a subverted borg.
        var emagged = EnsureComp<EmaggedComponent>(cyborg);
        emagged.EmagType |= EmagType.Interaction;
        Dirty(cyborg, emagged);

        // Ownership tracking for the Malf AI borg menu and objectives.
        if (malfAi != null && HasComp<MalfAiMarkerComponent>(malfAi.Value))
        {
            var ctrl = EnsureComp<MalfAiControlledComponent>(cyborg);
            ctrl.Controller = malfAi;
            ctrl.UniqueId ??= $"borg-{Guid.NewGuid():N}";
            Dirty(cyborg, ctrl);
        }
    }
}

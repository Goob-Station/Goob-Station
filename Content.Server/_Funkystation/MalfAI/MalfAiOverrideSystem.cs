// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: MIT

using Content.Server.Construction.Components;
using Content.Server.NPC.Components;
using Content.Server.NPC.HTN;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Weapons.Melee;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.CombatMode;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Silicons.StationAi;
using Content.Shared._Gabystation.MalfAi.Components;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI Override Machine ability.
/// Converts targeted machines into hostile mobile entities.
/// To note though, I'm not too big a fan of this ability. It's a little TOO silly for my taste.
/// That in mind I will be making this upgrade have more unique interactions based on the machine that you use it on in future.
/// </summary>
public sealed class MalfAiOverrideSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiHeldComponent, MalfAiOverrideMachineActionEvent>(OnOverrideMachine);
    }

    private void OnOverrideMachine(Entity<StationAiHeldComponent> ai, ref MalfAiOverrideMachineActionEvent args)
    {
        var popupTarget = GetAiEyeForPopup(ai.Owner) ?? ai.Owner;

        // Only malf AIs can use this.
        if (!HasComp<MalfunctioningAiComponent>(ai))
        {
            _popup.PopupEntity(Loc.GetString("malfai-override-not-malf"), popupTarget, ai);
            return;
        }

        // Get the target entity at the clicked coordinates.
        var coords = args.Target;
        var mapCoords = _xform.ToMapCoordinates(coords);

        if (mapCoords.MapId == MapId.Nullspace)
        {
            _popup.PopupEntity(Loc.GetString("malfai-override-invalid-location"), popupTarget, ai);
            return;
        }

        // Find entities at the target location.
        var entitiesAtLocation = _lookup.GetEntitiesInRange(mapCoords, 0.5f);
        EntityUid? targetMachine = null;

        foreach (var entity in entitiesAtLocation)
        {
            if (HasComp<MachineComponent>(entity))
            {
                targetMachine = entity;
                break;
            }
        }

        if (targetMachine == null)
        {
            _popup.PopupEntity(Loc.GetString("malfai-override-no-machine"), popupTarget, ai);
            return;
        }

        // Step 1: Unanchor the machine to make it mobile.
        _xform.Unanchor(targetMachine.Value);

        // Step 2: Make it hostile by adding NPC faction.
        var factionComp = EnsureComp<NpcFactionMemberComponent>(targetMachine.Value);
        _npcFaction.AddFaction(targetMachine.Value, "SimpleHostile");

        // Add HTN component for AI behavior.
        var htnComp = EnsureComp<HTNComponent>(targetMachine.Value);
        htnComp.RootTask = new HTNCompoundTask() { Task = "SimpleHostileCompound" };
        htnComp.Blackboard.SetValue("CurrentEntities", new HashSet<EntityUid>());

        // Step 3: Add movement components for AI mobility.
        EnsureComp<InputMoverComponent>(targetMachine.Value);
        EnsureComp<MobMoverComponent>(targetMachine.Value);

        // Add movement speed modifier for proper NPC movement.
        EnsureComp<MovementSpeedModifierComponent>(targetMachine.Value);
        _movementSpeed.ChangeBaseSpeed(targetMachine.Value, 2.5f, 4.0f, 20f);

        // Add combat mode component to handle combat interactions.
        EnsureComp<CombatModeComponent>(targetMachine.Value);

        // Step 4: Add melee weapon component for damage dealing.
        var meleeComp = EnsureComp<MeleeWeaponComponent>(targetMachine.Value);
        meleeComp.Damage = new DamageSpecifier
        {
            DamageDict = new Dictionary<string, FixedPoint2>
            {
                { "Blunt", 15 }
            }
        };
        meleeComp.Range = 1.5f;
        meleeComp.AttackRate = 1.0f;

        // Add NPC melee combat component.
        EnsureComp<NPCMeleeCombatComponent>(targetMachine.Value);

        _popup.PopupEntity(Loc.GetString("malfai-override-success"), popupTarget, ai);
        args.Handled = true;
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable.
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }
}

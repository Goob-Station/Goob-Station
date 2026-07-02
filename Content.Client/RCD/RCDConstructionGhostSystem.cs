// SPDX-FileCopyrightText: 2024 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Hands.Systems;
using Content.Client._Pirate.RCD; // Pirate: chem plumbing
using Content.Shared.Atmos.Components; // Pirate: chem plumbing
using Content.Shared.Hands.Components;
using Content.Shared.Input;
using Content.Shared.Interaction;
using Content.Shared.RCD;
using Content.Shared.RCD.Components;
using Robust.Client.Placement;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;


namespace Content.Client.RCD;

/// <summary>
/// System for handling structure ghost placement in places where RCD can create objects.
/// </summary>
public sealed class RCDConstructionGhostSystem : EntitySystem
{
    private const string PlacementMode = nameof(AlignRCDConstruction);
    private const string RpdPlacementMode = nameof(AlignRPDAtmosPipeLayers); // Pirate: chem plumbing

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPlacementManager _placementManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    private Direction _placementDirection = default;
    private EntityUid? _lastHeldRcd; // Pirate: chem plumbing
    private bool _useMirrorPrototype = false;
    public event EventHandler? FlipConstructionPrototype;

    public override void Initialize()
    {
        base.Initialize();

        // bind key
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.EditorFlipObject,
                new PointerInputCmdHandler(HandleFlip, outsidePrediction: true))
            .Register<RCDConstructionGhostSystem>();
    }

    public override void Shutdown()
    {
        CommandBinds.Unregister<RCDConstructionGhostSystem>();
        base.Shutdown();
    }

    private bool HandleFlip(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        if (args.State == BoundKeyState.Down)
        {
            if (!_placementManager.IsActive || _placementManager.Eraser)
                return false;

            var placerEntity = _placementManager.CurrentPermission?.MobUid;

            if (!TryComp<RCDComponent>(placerEntity, out var rcd))
                return false;

            var prototype = _protoManager.Index(rcd.ProtoId);
            if (string.IsNullOrEmpty(prototype.MirrorPrototype))
                return false;

            _useMirrorPrototype = !rcd.UseMirrorPrototype;

            var useProto = _useMirrorPrototype ? prototype.MirrorPrototype : prototype.Prototype;
            CreatePlacer(placerEntity.Value, useProto, prototype.Mode, GetPlacementMode(rcd, prototype)); // Pirate: chem plumbing

            // tell the server

            RaiseNetworkEvent(new RCDConstructionGhostFlipEvent(GetNetEntity(placerEntity.Value), _useMirrorPrototype));
        }

        return true;
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Get current placer data
        var placerEntity = _placementManager.CurrentPermission?.MobUid;
        var placerProto = _placementManager.CurrentPermission?.EntityType;
        var placerIsRCD = HasComp<RCDComponent>(placerEntity);

        // Exit if erasing or the current placer is not an RCD (build mode is active)
        if (_placementManager.Eraser || (placerEntity != null && !placerIsRCD))
            return;

        // Determine if player is carrying an RCD in their active hand
        if (_playerManager.LocalSession?.AttachedEntity is not { } player)
            return;

        var heldEntity = _hands.GetActiveItem(player);

        #region Pirate: chem plumbing
        if (heldEntity != null && IsClientSide(heldEntity.Value))
            return;
        #endregion

        if (!TryComp<RCDComponent>(heldEntity, out var rcd))
        {
            // If the player was holding an RCD, but is no longer, cancel placement
            if (placerIsRCD)
                _placementManager.Clear();

            _lastHeldRcd = null; // Pirate: chem plumbing
            return;
        }
        #region Pirate: chem plumbing
        var prototype = _protoManager.Index(rcd.ProtoId);
        var useProto = (_useMirrorPrototype && !string.IsNullOrEmpty(prototype.MirrorPrototype)) ? prototype.MirrorPrototype : prototype.Prototype;
        var isLayered = (rcd.IsRpd || rcd.IsRPLD) && prototype.HasLayers;
        var desiredMode = GetPlacementMode(rcd, prototype);
        #endregion

        #region Pirate: chem plumbing
        if (_lastHeldRcd != heldEntity)
        {
            _lastHeldRcd = heldEntity;
            _placementDirection = _placementManager.Direction;
            RaiseNetworkEvent(new RCDConstructionGhostRotationEvent(GetNetEntity(heldEntity.Value), _placementDirection));
        }
        #endregion
        // Update the direction the RCD prototype based on the placer direction
        else if (_placementDirection != _placementManager.Direction) // Pirate: chem plumbing
        {
            _placementDirection = _placementManager.Direction;
            RaiseNetworkEvent(new RCDConstructionGhostRotationEvent(GetNetEntity(heldEntity.Value), _placementDirection));
        }
        // If the placer has not changed build it.
        #region Pirate: chem plumbing
        if (heldEntity != placerEntity ||
            _placementManager.CurrentPermission?.PlacementOption != desiredMode ||
            !PrototypeMatchesCurrentPlacer(useProto, placerProto, isLayered))
        #endregion
        {
            CreatePlacer(heldEntity.Value, useProto, prototype.Mode, desiredMode); // Pirate: chem plumbing
        }


    }

    private void CreatePlacer(EntityUid uid, string? prototype, RcdMode mode, string placementMode) // Pirate: chem plumbing
    {
        // Create a new placer
        var newObjInfo = new PlacementInformation
        {
            MobUid = uid,
            PlacementOption = placementMode, // Pirate: chem plumbing
            EntityType = prototype,
            Range = (int) Math.Ceiling(SharedInteractionSystem.InteractionRange),
            IsTile = (mode == RcdMode.ConstructTile),
            UseEditorContext = false,
        };

        _placementManager.Clear();
        _placementManager.BeginPlacing(newObjInfo);
    }

    #region Pirate: chem plumbing
    private static string GetPlacementMode(RCDComponent component, RCDPrototype prototype)
    {
        return (component.IsRpd || component.IsRPLD) && prototype.HasLayers
            ? RpdPlacementMode
            : PlacementMode;
    }

    private bool PrototypeMatchesCurrentPlacer(string? expectedProto, string? currentProto, bool isLayered)
    {
        if (expectedProto == currentProto)
            return true;

        if (!isLayered || expectedProto == null || currentProto == null)
            return false;

        if (!_protoManager.TryIndex<EntityPrototype>(expectedProto, out var entityProto))
            return false;

        if (!entityProto.TryGetComponent<AtmosPipeLayersComponent>(out var atmosPipeLayers, EntityManager.ComponentFactory))
            return false;

        foreach (var alternative in atmosPipeLayers.AlternativePrototypes.Values)
        {
            if (alternative.Id == currentProto)
                return true;
        }

        return false;
    }
    #endregion
}

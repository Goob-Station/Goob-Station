using System.Numerics;
using Content.Shared._Pirate.Plumbing;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Client.SubFloor;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Shared.Map;

namespace Content.Client._Pirate.Plumbing;

/// <summary>
///     Client system that creates and updates plumbing connector sprite layers.
///     Regular machines use one connector layer per cardinal direction.
///     Manifolds use slot-based connector layers per side.
///     Layers hide when covered by floor tiles (server sends CoveredByFloor state).
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingConnectorAppearanceSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SpriteSystem _sprite = default!;

    private static readonly Color _inletColor = new(1.0f, 0.35f, 0.35f);  // Vibrant Red
    private static readonly Color _outletColor = new(0.35f, 0.6f, 1.0f);  // Vibrant Blue
    private static readonly Color _mixingInletColor = new(0.35f, 0.9f, 0.35f);  // Vibrant Green
    private static readonly PlumbingConnectionLayer[] _connectionLayers = Enum.GetValues<PlumbingConnectionLayer>();
    private static readonly PipeDirection[] _cardinalDirections =
    [
        PipeDirection.North,
        PipeDirection.South,
        PipeDirection.East,
        PipeDirection.West,
    ];

    private static readonly ManifoldSideSpec[] _manifoldSides =
    [
        new(PipeDirection.North, 3),
        new(PipeDirection.South, 3),
    ];

    private const float ManifoldSlotSpacing = 0.0625f;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<PlumbingConnectorAppearanceComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PlumbingConnectorAppearanceComponent, AppearanceChangeEvent>(OnAppearanceChanged, after: [typeof(SubFloorHideSystem)]);
    }

    private void OnInit(EntityUid uid, PlumbingConnectorAppearanceComponent component, ComponentInit args)
    {
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        EnsureConnectorLayers(uid, component, sprite);

        if (component.PreviewNodeDirections != PipeDirection.None && IsPlacementPreview(uid))
            ApplyPlacementPreview(uid, component, sprite);
    }

    private void EnsureConnectorLayers(EntityUid uid, PlumbingConnectorAppearanceComponent component, SpriteComponent sprite)
    {
        foreach (var layerKey in _connectionLayers)
        {
            var layerName = layerKey.ToString();
            if (_sprite.LayerMapTryGet((uid, sprite), layerName, out _, false))
                continue;

            var direction = (PipeDirection) layerKey;
            var baseOffset = GetDirectionOffset(direction, component.Offset);

            _sprite.AddBlankLayer((uid, sprite), 0);
            _sprite.LayerMapSet((uid, sprite), layerName, 0);

            _sprite.LayerSetRsi((uid, sprite), 0, component.Disconnected.RsiPath);
            _sprite.LayerSetRsiState((uid, sprite), 0, component.Disconnected.RsiState);
            _sprite.LayerSetDirOffset((uid, sprite), 0, ToOffset(direction));
            _sprite.LayerSetOffset((uid, sprite), 0, baseOffset);
            _sprite.LayerSetVisible((uid, sprite), 0, false);
        }
    }

    private static Vector2 GetDirectionOffset(PipeDirection direction, float offset)
        => direction.ToDirection().ToVec() * offset;

    public void ApplyPlacementPreview(EntityUid uid, PlumbingConnectorAppearanceComponent component, SpriteComponent sprite)
    {
        EnsureConnectorLayers(uid, component, sprite);

        foreach (var layerKey in _connectionLayers)
        {
            var dir = (PipeDirection) layerKey;
            var layerName = layerKey.ToString();
            if (!_sprite.LayerMapTryGet((uid, sprite), layerName, out var layer, false))
                continue;

            var hasNode = component.PreviewNodeDirections.HasDirection(dir);
            sprite[layer].Visible = hasNode;
            if (!hasNode)
                continue;

            _sprite.LayerSetRsiState((uid, sprite), layer, component.Disconnected.RsiState);
            _sprite.LayerSetOffset((uid, sprite), layer, GetDirectionOffset(dir, component.Offset));
            sprite[layer].Color = GetConnectorColor(
                component.PreviewInletDirections.HasDirection(dir),
                component.PreviewOutletDirections.HasDirection(dir),
                component.PreviewMixingInletDirections.HasDirection(dir));
        }
    }

    private void OnAppearanceChanged(EntityUid uid, PlumbingConnectorAppearanceComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!args.Sprite.Visible)
            return;

        if (IsManifoldAppearance(uid, args.Component))
        {
            HideAllLayers(uid, args.Sprite);
            EnsureManifoldLayers(uid, component, args.Sprite);
            UpdateManifoldAppearance(uid, component, ref args);
            return;
        }

        // Hide if no nodes exists somehow
        if (!_appearance.TryGetData<int>(uid, PlumbingVisuals.NodeDirections, out var nodeDirectionsInt, args.Component))
        {
            if (component.PreviewNodeDirections != PipeDirection.None && IsPlacementPreview(uid))
                ApplyPlacementPreview(uid, component, args.Sprite);
            else
                HideAllLayers(uid, args.Sprite);

            return;
        }

        if (!_appearance.TryGetData<int>(uid, PlumbingVisuals.ConnectedDirections, out var connectedDirectionsInt, args.Component))
            connectedDirectionsInt = 0;

        if (!_appearance.TryGetData<int>(uid, PlumbingVisuals.ConnectedLayerByDirection, out var connectedLayersPacked, args.Component))
            connectedLayersPacked = 0;

        if (!_appearance.TryGetData<int>(uid, PlumbingVisuals.InletDirections, out var inletDirectionsInt, args.Component))
            inletDirectionsInt = 0;
        if (!_appearance.TryGetData<int>(uid, PlumbingVisuals.OutletDirections, out var outletDirectionsInt, args.Component))
            outletDirectionsInt = 0;
        if (!_appearance.TryGetData<int>(uid, PlumbingVisuals.MixingInletDirections, out var mixingInletDirectionsInt, args.Component))
            mixingInletDirectionsInt = 0;

        if (!_appearance.TryGetData<bool>(uid, PlumbingVisuals.CoveredByFloor, out var coveredByFloor, args.Component))
            coveredByFloor = false;

        if (!_appearance.TryGetData<int>(uid, PlumbingVisuals.CoveredDirections, out var coveredDirectionsInt, args.Component))
            coveredDirectionsInt = 0;

        var nodeDirections = (PipeDirection)nodeDirectionsInt;
        var connectedDirections = (PipeDirection)connectedDirectionsInt;
        var inletDirections = (PipeDirection)inletDirectionsInt;
        var outletDirections = (PipeDirection)outletDirectionsInt;
        var mixingInletDirections = (PipeDirection)mixingInletDirectionsInt;
        var coveredDirections = (PipeDirection)coveredDirectionsInt;

        // Get the entity's local rotation to transform world directions to local
        if (!_xformQuery.TryGetComponent(uid, out var xform))
            return;
        var localRotation = xform.LocalRotation;
        var connectedDirectionsLocal = connectedDirections.RotatePipeDirection(-localRotation);
        var nodeDirectionsLocal = nodeDirections.RotatePipeDirection(-localRotation);
        var inletDirectionsLocal = inletDirections.RotatePipeDirection(-localRotation);
        var outletDirectionsLocal = outletDirections.RotatePipeDirection(-localRotation);
        var mixingInletDirectionsLocal = mixingInletDirections.RotatePipeDirection(-localRotation);
        var coveredDirectionsLocal = coveredDirections.RotatePipeDirection(-localRotation);

        foreach (var layerKey in _connectionLayers)
        {
            var dir = (PipeDirection)layerKey;
            var hasNode = nodeDirectionsLocal.HasDirection(dir);
            var isConnected = connectedDirectionsLocal.HasDirection(dir);
            var isInlet = inletDirectionsLocal.HasDirection(dir);
            var isOutlet = outletDirectionsLocal.HasDirection(dir);
            var isMixingInlet = mixingInletDirectionsLocal.HasDirection(dir);
            var isCovered = coveredByFloor || coveredDirectionsLocal.HasDirection(dir);

            var color = GetConnectorColor(isInlet, isOutlet, isMixingInlet);

            var layerName = layerKey.ToString();
            if (_sprite.LayerMapTryGet((uid, args.Sprite), layerName, out var layerKey2, false))
            {
                var layer = args.Sprite[layerKey2];
                layer.Visible = hasNode && !isCovered;

                if (layer.Visible)
                {
                    // Swap sprite based on connection state
                    if (isConnected)
                    {
                        _sprite.LayerSetRsiState((uid, args.Sprite), layerKey2, component.Connected.RsiState);
                        var worldDirection = dir.RotatePipeDirection(localRotation);
                        var connectedLayer = GetConnectedLayer(connectedLayersPacked, worldDirection);
                        _sprite.LayerSetOffset((uid, args.Sprite), layerKey2, GetConnectedLayerOffset(worldDirection, localRotation, connectedLayer, component.Offset));
                    }
                    else
                    {
                        _sprite.LayerSetRsiState((uid, args.Sprite), layerKey2, component.Disconnected.RsiState);
                        _sprite.LayerSetOffset((uid, args.Sprite), layerKey2, GetDirectionOffset(dir, component.Offset));
                    }
                    layer.Color = color;
                }
            }
        }
    }

    private void UpdateManifoldAppearance(EntityUid uid, PlumbingConnectorAppearanceComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not { } sprite)
            return;

        if (!_appearance.TryGetData<int>(uid, PlumbingVisuals.ManifoldConnectedSlotsByDirection, out var connectedSlotsPacked, args.Component))
            connectedSlotsPacked = 0;

        if (!_appearance.TryGetData<bool>(uid, PlumbingVisuals.CoveredByFloor, out var coveredByFloor, args.Component))
            coveredByFloor = false;

        if (!_xformQuery.TryGetComponent(uid, out var xform))
            return;

        var localRotation = xform.LocalRotation;
        var localPacked = RotateManifoldSlotsToLocal(connectedSlotsPacked, localRotation);

        foreach (var (direction, slotIndex, slotCount, layerName) in EnumerateManifoldLayers())
        {
            if (!_sprite.LayerMapTryGet((uid, sprite), layerName, out var layerKey, false))
                continue;

            var layer = sprite[layerKey];
            var slotMask = ReadPackedDirectionNibble(localPacked, direction);
            var isConnected = (slotMask & (1 << slotIndex)) != 0;
            layer.Visible = isConnected && !coveredByFloor;
            if (!layer.Visible)
                continue;

            _sprite.LayerSetRsiState((uid, sprite), layerKey, component.Connected.RsiState);
            const float ForwardOffset = 0f;
            _sprite.LayerSetOffset((uid, sprite), layerKey,
                GetManifoldSlotOffset(direction, slotIndex, slotCount, ForwardOffset, ManifoldSlotSpacing, component.Offset, localRotation));
            layer.Color = Color.White;
        }
    }

    private bool IsManifoldAppearance(EntityUid uid, AppearanceComponent? appearance)
    {
        if (_appearance.TryGetData<bool>(uid, PlumbingVisuals.ManifoldMode, out var manifoldMode, appearance))
            return manifoldMode;

        return _appearance.TryGetData<int>(uid, PlumbingVisuals.ManifoldConnectedSlotsByDirection, out _, appearance);
    }

    private void EnsureManifoldLayers(EntityUid uid, PlumbingConnectorAppearanceComponent component, SpriteComponent sprite)
    {
        foreach (var (direction, slotIndex, slotCount, layerName) in EnumerateManifoldLayers())
        {
            if (_sprite.LayerMapTryGet((uid, sprite), layerName, out _, false))
                continue;

            _sprite.AddBlankLayer((uid, sprite), 0);
            _sprite.LayerMapSet((uid, sprite), layerName, 0);
            _sprite.LayerSetRsi((uid, sprite), 0, component.Disconnected.RsiPath);
            _sprite.LayerSetRsiState((uid, sprite), 0, component.Disconnected.RsiState);
            _sprite.LayerSetDirOffset((uid, sprite), 0, ToOffset(direction));
            _sprite.LayerSetOffset((uid, sprite), 0, GetManifoldSlotOffset(direction, slotIndex, slotCount, 0f, ManifoldSlotSpacing, component.Offset, Angle.Zero));
            _sprite.LayerSetVisible((uid, sprite), 0, false);
        }
    }

    private static IEnumerable<(PipeDirection Direction, int SlotIndex, int SlotCount, string LayerName)> EnumerateManifoldLayers()
    {
        foreach (var side in _manifoldSides)
        {
            for (var slotIndex = 0; slotIndex < side.SlotCount; slotIndex++)
            {
                yield return (side.Direction, slotIndex, side.SlotCount, $"{side.Direction}_slot_{slotIndex}");
            }
        }
    }

    private void HideAllLayers(EntityUid uid, SpriteComponent sprite)
    {
        foreach (var layerKey in _connectionLayers)
        {
            var layerName = layerKey.ToString();
            if (_sprite.LayerMapTryGet((uid, sprite), layerName, out var key, false))
                sprite[key].Visible = false;
        }
    }

    private bool IsPlacementPreview(EntityUid uid)
        => TryComp(uid, out TransformComponent? xform) && xform.MapID == MapId.Nullspace;

    private static AtmosPipeLayer GetConnectedLayer(int packedData, PipeDirection direction)
    {
        var encoded = ReadPackedDirectionNibble(packedData, direction);
        if (encoded == 0)
            return AtmosPipeLayer.Primary;

        return (AtmosPipeLayer) Math.Clamp(encoded - 1, 0, (int) AtmosPipeLayer.Tertiary);
    }

    private static Vector2 GetConnectedLayerOffset(PipeDirection worldDirection, Angle localRotation, AtmosPipeLayer layer, float offset)
    {
        var sidewaysOffset = offset + (1f / 32f);

        var worldOffset = layer switch
        {
            AtmosPipeLayer.Secondary => GetPerpendicularOffset(worldDirection, sidewaysOffset),
            AtmosPipeLayer.Tertiary => GetPerpendicularOffset(worldDirection, -sidewaysOffset),
            _ => Vector2.Zero,
        };

        return (-localRotation).RotateVec(worldOffset);
    }

    private static Vector2 GetPerpendicularOffset(PipeDirection direction, float magnitude)
        => direction switch
        {
            PipeDirection.North or PipeDirection.South => new Vector2(magnitude, 0f),
            PipeDirection.East or PipeDirection.West => new Vector2(0f, magnitude),
            _ => Vector2.Zero,
    };

    private static int RotateManifoldSlotsToLocal(int packedData, Angle localRotation)
    {
        var rotated = 0;
        foreach (var worldDirection in _cardinalDirections)
        {
            var mask = ReadPackedDirectionNibble(packedData, worldDirection);
            if (mask == 0)
                continue;

            var localDirection = worldDirection.RotatePipeDirection(-localRotation);
            rotated = WritePackedDirectionNibble(rotated, localDirection, mask);
        }

        return rotated;
    }

    private static int ReadPackedDirectionNibble(int packedData, PipeDirection direction)
    {
        var shift = GetDirectionNibbleShift(direction);

        if (shift < 0)
            return 0;

        return (packedData >> shift) & 0xF;
    }

    private static int WritePackedDirectionNibble(int packedData, PipeDirection direction, int nibble)
    {
        var shift = GetDirectionNibbleShift(direction);

        if (shift < 0)
            return packedData;

        var clearMask = ~(0xF << shift);
        return (packedData & clearMask) | ((nibble & 0xF) << shift);
    }

    private static int GetDirectionNibbleShift(PipeDirection direction)
        => direction switch
        {
            PipeDirection.North => 0,
            PipeDirection.South => 4,
            PipeDirection.East => 8,
            PipeDirection.West => 12,
            _ => -1,
    };

    private static Vector2 GetManifoldSlotOffset(PipeDirection direction, int slotIndex, int slotCount, float baseOffset, float slotSpacing, float connectorOffset, Angle localRotation)
    {
        var baseDirectionOffset = direction.ToDirection().ToVec() * baseOffset;

        if (slotCount == 3)
        {
            var slotLayer = slotIndex switch
            {
                0 => AtmosPipeLayer.Tertiary,
                1 => AtmosPipeLayer.Primary,
                2 => AtmosPipeLayer.Secondary,
                _ => AtmosPipeLayer.Primary,
            };

            var worldDirection = direction.RotatePipeDirection(localRotation);
            var layerOffset = GetConnectedLayerOffset(worldDirection, localRotation, slotLayer, connectorOffset);
            return baseDirectionOffset + layerOffset;
        }

        var centeredIndex = slotIndex - ((slotCount - 1) / 2f);
        var spread = centeredIndex * slotSpacing;

        var perpendicularOffset = direction switch
        {
            PipeDirection.North or PipeDirection.South => new Vector2(spread, 0f),
            PipeDirection.East or PipeDirection.West => new Vector2(0f, spread),
            _ => Vector2.Zero,
        };

        return baseDirectionOffset + perpendicularOffset;
    }

    private static Color GetConnectorColor(bool isInlet, bool isOutlet, bool isMixingInlet)
    {
        if (isMixingInlet)
            return _mixingInletColor;

        if (isInlet)
            return _inletColor;

        if (isOutlet)
            return _outletColor;

        return Color.White;
    }

    private SpriteComponent.DirectionOffset ToOffset(PipeDirection direction)
        => direction switch
        {
            PipeDirection.North => SpriteComponent.DirectionOffset.Flip,
            PipeDirection.East => SpriteComponent.DirectionOffset.CounterClockwise,
            PipeDirection.West => SpriteComponent.DirectionOffset.Clockwise,
            _ => SpriteComponent.DirectionOffset.None,
    };

    private enum PlumbingConnectionLayer : byte
    {
        NorthConnection = PipeDirection.North,
        SouthConnection = PipeDirection.South,
        EastConnection = PipeDirection.East,
        WestConnection = PipeDirection.West,
    }

    private readonly record struct ManifoldSideSpec(PipeDirection Direction, int SlotCount);
}

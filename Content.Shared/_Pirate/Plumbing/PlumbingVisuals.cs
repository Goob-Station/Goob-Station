using Robust.Shared.Serialization;

namespace Content.Shared._Pirate.Plumbing;

[Serializable, NetSerializable]
public enum PlumbingVisuals : byte
{
    /// <summary>
    ///     Whether the plumbing machine is actively running.
    /// </summary>
    Running,

    /// <summary>
    ///     Directions that have PlumbingNodes defined.
    ///     Used to show jagged connectors in those directions.
    /// </summary>
    NodeDirections,

    /// <summary>
    ///     Directions that are connected.
    ///     Used to show smooth connector layer instead when connected.
    /// </summary>
    ConnectedDirections,

    /// <summary>
    ///     Packed connected layer index per cardinal direction.
    ///     Stores (layer + 1) in 4-bit nibbles, where 0 means no layer assigned.
    /// </summary>
    ConnectedLayerByDirection,

    /// <summary>
    ///     Directions that have inlet nodes.
    ///     Used to color inlet connectors red.
    /// </summary>
    InletDirections,

    /// <summary>
    ///     Directions that have outlet nodes.
    ///     Used to color outlet connectors blue.
    /// </summary>
    OutletDirections,

    /// <summary>
    ///     Whether the entity is covered by a floor tile.
    ///     Used to hide connector layers under floors without using SubFloorHide.
    /// </summary>
    CoveredByFloor,

    /// <summary>
    ///     Directions that have mixing inlet nodes.
    ///     Used to color mixing inlet connectors green.
    /// </summary>
    MixingInletDirections,

    /// <summary>
    ///     Whether this entity should use manifold connector rendering instead of regular per-direction connector layers.
    /// </summary>
    ManifoldMode,

    /// <summary>
    ///     Packed connected slot bitmask per cardinal direction for manifold-mode connectors.
    ///     Uses 4-bit nibbles per direction, where each bit is a slot index (0-3).
    /// </summary>
    ManifoldConnectedSlotsByDirection,

    /// <summary>
    ///     Directions whose neighbouring tile is covered by a floor.
    ///     Connector stubs pointing into a covered tile hide along with the ducts under
    ///     that floor, so a machine on exposed plating doesn't leave pipes poking over
    ///     adjacent floor tiles.
    /// </summary>
    CoveredDirections
}

[Serializable, NetSerializable]
public enum PlumbingVisualLayers : byte
{
    Base,
    Overlay
}

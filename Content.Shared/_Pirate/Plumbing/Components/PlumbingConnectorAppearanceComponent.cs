using Robust.Shared.Utility;
using Content.Shared.Atmos;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
/// Adds dynamic connector sprites to plumbing machines.
/// Shows jagged connectors where nodes exist and switches layer to smooth ends version when connected.
/// </summary>
[RegisterComponent]
public sealed partial class PlumbingConnectorAppearanceComponent : Component
{
    /// <summary>
    /// Sprite for disconnected (jagged) connectors.
    /// </summary>
    [DataField]
    public SpriteSpecifier.Rsi Disconnected = new(new("_Pirate/Structures/Piping/Plumbing/plumbers.rsi"), "ductConnector");

    /// <summary>
    /// Sprite for connected (smooth) connectors - overlays disconnected state.
    /// </summary>
    [DataField]
    public SpriteSpecifier.Rsi Connected = new(new("_Pirate/Structures/Piping/Plumbing/plumbers.rsi"), "ductConnector_connected");

    /// <summary>
    /// Offset from center for connector sprites. Used so jagged ends stick out from under a machine to be visible under big sprites.
    /// </summary>
    [DataField]
    public float Offset;

    /// <summary>
    ///     Local connector directions to show on client-side placement previews before server appearance data exists.
    /// </summary>
    [DataField]
    public PipeDirection PreviewNodeDirections = PipeDirection.None;

    /// <summary>
    ///     Local inlet connector directions to color on placement previews.
    /// </summary>
    [DataField]
    public PipeDirection PreviewInletDirections = PipeDirection.None;

    /// <summary>
    ///     Local outlet connector directions to color on placement previews.
    /// </summary>
    [DataField]
    public PipeDirection PreviewOutletDirections = PipeDirection.None;

    /// <summary>
    ///     Local mixing inlet connector directions to color on placement previews.
    /// </summary>
    [DataField]
    public PipeDirection PreviewMixingInletDirections = PipeDirection.None;
}

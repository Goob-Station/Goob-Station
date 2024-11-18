using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._White.FootPrint;

[RegisterComponent]
public sealed partial class FootPrintsComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public ResPath RsiPath = new("/Textures/_White/Effects/footprints.rsi");

    // all of those are set as a layer
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public string LeftBarePrint = "footprint-left-bare-human";

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public string RightBarePrint = "footprint-right-bare-human";

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public string ShoesPrint = "footprint-shoes";

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public string SuitPrint = "footprint-suit";

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public string[] DraggingPrint =
    [
        "dragging-1",
        "dragging-2",
        "dragging-3",
        "dragging-4",
        "dragging-5",
    ];
    // yea, those

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public EntProtoId<FootPrintComponent> StepProtoId = "Footstep";

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public Color PrintsColor = Color.FromHex("#00000000");

    /// <summary>
    /// The size scaling factor for footprint steps. Must be positive.
    /// </summary>
    [DataField]
    public float StepSize = 0.7f;

    /// <summary>
    /// The size scaling factor for drag marks. Must be positive.
    /// </summary>
    [DataField]
    public float DragSize = 0.5f;

    /// <summary>
    /// The amount of color to transfer from the source (e.g., puddle) to the footprint.
    /// </summary>
    [DataField]
    public float ColorQuantity;

    /// <summary>
    /// The factor by which the alpha channel is reduced in subsequent footprints.
    /// </summary>
    [DataField]
    public float ColorReduceAlpha = 0.1f;

    [DataField]
    public string? ReagentToTransfer;

    [DataField]
    public Vector2 OffsetPrint = new(0.1f, 0f);

    /// <summary>
    /// Tracks which foot should make the next print. True for right foot, false for left.
    /// </summary>
    public bool RightStep = true;

    /// <summary>
    /// The position of the last footprint in world coordinates.
    /// </summary>
    public Vector2 StepPos = Vector2.Zero;

    /// <summary>
    /// Controls how quickly the footprint color transitions between steps.
    /// Value between 0 and 1, where higher values mean faster color changes.
    /// </summary>
    public float ColorInterpolationFactor = 0.2f;
}

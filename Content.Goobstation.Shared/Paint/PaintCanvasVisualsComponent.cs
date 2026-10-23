using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Paint;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class PaintCanvasVisualsComponent : Component
{
    [DataField]
    public ResPath BackingSprite = new("_Goobstation/Objects/Fun/canvas.rsi");

    [DataField]
    public ResPath FrameSprite = new("_Goobstation/Objects/Fun/painting_frames.rsi");

    [DataField, AutoNetworkedField]
    public string SizeKey = "24x24";

    [DataField, AutoNetworkedField]
    public string BackingState = "canvas_24x24";

    [DataField, AutoNetworkedField]
    public Vector2 PaintScale = new(0.375f, 0.375f);

    [DataField, AutoNetworkedField]
    public Vector2 PaintOffset = Vector2.Zero;

    [DataField, AutoNetworkedField]
    public Vector2 EaselOffset = Vector2.Zero;

    [DataField, AutoNetworkedField]
    public string? Frame;
}

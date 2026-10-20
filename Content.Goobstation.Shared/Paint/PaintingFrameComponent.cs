using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Paint;

[RegisterComponent, NetworkedComponent]
public sealed partial class PaintingFrameComponent : Component
{
    [DataField(required: true)]
    public string Material = string.Empty;
}

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Paint;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PaintToolComponent : Component
{
    [DataField, AutoNetworkedField]
    public PaintColorMode ColorMode = PaintColorMode.Fixed;

    [DataField, AutoNetworkedField]
    public Color Color = Color.Black;

    [DataField, AutoNetworkedField]
    public int MinBrushSize = 1;

    [DataField, AutoNetworkedField]
    public int MaxBrushSize = 3;

    [DataField, AutoNetworkedField]
    public bool CanErase;

    [DataField]
    public int PixelsPerCharge;

    [DataField]
    public string? Solution;

    [DataField]
    public int PixelsPerUnit = 128;

    [ViewVariables]
    public int PixelDebt;
}

[Serializable, NetSerializable]
public enum PaintColorMode : byte
{
    Fixed,
    Picker,
    Crayon,
    Solution,
}

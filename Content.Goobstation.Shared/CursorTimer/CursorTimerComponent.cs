using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.CursorTimer;

/// <summary>
/// Shows a countdown next to the owners cursor until <see cref="EndTime"/>.
/// Add it with <see cref="CursorTimerSystem.SetTimer"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class CursorTimerComponent : Component
{
    [ViewVariables, AutoNetworkedField, AutoPausedField]
    public TimeSpan EndTime;

    [DataField, AutoNetworkedField]
    public string FontPath = "/Fonts/NotoSans/NotoSans-Bold.ttf";

    [DataField, AutoNetworkedField]
    public int FontSize = 12;

    [DataField, AutoNetworkedField]
    public Vector2 Offset = new(14f, -26f);

    [DataField, AutoNetworkedField]
    public Color Color = Color.White;

    [DataField, AutoNetworkedField]
    public Color WarningColor = Color.Red;

    [DataField, AutoNetworkedField]
    public float WarningSeconds = 3f;

    [DataField, AutoNetworkedField]
    public int Decimals = 1;
}

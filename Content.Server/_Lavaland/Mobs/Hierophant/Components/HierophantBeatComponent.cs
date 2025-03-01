using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantBeatComponent : Component
{
    [DataField]
    public float MovementSpeedBuff = 1.25f;

    [DataField]
    public string HierophantBeatAlertKey = "HierophantBeat";
}

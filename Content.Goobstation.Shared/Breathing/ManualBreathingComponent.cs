using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Breathing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ManualBreathingComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool NeedsInhale;

    [DataField, AutoNetworkedField]
    public TimeSpan LastManualBreathTime;

    [DataField, AutoNetworkedField]
    public float BreathCooldown = 15f;

    [DataField, AutoNetworkedField]
    public float BlurStartTime = 15f;

    [DataField, AutoNetworkedField]
    public float BlurGrowthRate = 0.15f;
}

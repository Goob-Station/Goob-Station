using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// This is used for mob growth between baby, adult etc...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MobGrowthComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public float HungerRequired = 100f;

    [DataField(required: true), AutoNetworkedField]
    public string CurrentStage = string.Empty;

    [DataField(required: true), AutoNetworkedField]
    public List<string> Stages = new List<string>();

}

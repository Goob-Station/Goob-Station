using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Voidwalker.Objectives.Components;

[RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
public sealed partial class VoidwalkerKidnapConditionComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Kidnapped;
}

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class AltClothingLayerComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool AltStyle;

    [DataField(required: true)]
    public string DefaultLayer;

    [DataField(required: true)]
    public string AltLayer;

    [DataField(required: true)]
    public LocId ChangeToAltMessage;

    [DataField(required: true)]
    public LocId ChangeToDefaultMessage;
}

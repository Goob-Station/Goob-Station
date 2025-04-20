namespace Content.Goobstation.Client.Clothing.Components;

[RegisterComponent]
public sealed partial class HideClothingLayerClothingComponent : Component
{
    [DataField(required: true)]
    public HashSet<string> HiddenSlots = new();
}

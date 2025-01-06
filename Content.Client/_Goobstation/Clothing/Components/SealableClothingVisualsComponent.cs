namespace Content.Client._Goobstation.Clothing.Components;

[RegisterComponent]
public sealed partial class SealableClothingVisualsComponent : Component
{
    [DataField]
    public string SpriteLayer = "sealed";

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> VisualLayers = new();
}

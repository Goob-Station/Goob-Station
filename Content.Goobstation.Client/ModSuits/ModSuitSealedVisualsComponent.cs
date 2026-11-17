namespace Content.Goobstation.Client.ModSuits;

[RegisterComponent]
public sealed partial class ModSuitSealedVisualsComponent : Component
{
    [DataField]
    public string SpriteLayer = "sealed";

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new();
}

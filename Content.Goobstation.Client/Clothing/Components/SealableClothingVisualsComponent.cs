// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Client.Clothing.Components;

[RegisterComponent]
public sealed partial class SealableClothingVisualsComponent : Component
{
    [DataField]
    public string SpriteLayer = "sealed";

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new(); //just use ClothingVisuals like anything else
}

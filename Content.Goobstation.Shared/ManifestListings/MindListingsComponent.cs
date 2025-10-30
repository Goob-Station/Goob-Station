using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.ManifestListings;

[RegisterComponent, NetworkedComponent]
public sealed partial class MindListingsComponent : Component
{
    [DataField]
    public Dictionary<int, List<ListingData>> Listings = new();

    [DataField]
    public SpriteSpecifier.Texture DefaultTexture = new(new ResPath("/Textures/Interface/Actions/shop.png"));
}

namespace Content.Goobstation.Shared.Spy;

[RegisterComponent]
public sealed partial class SpyBountyDatabaseComponent : Component
{
    [ViewVariables]
    public List<SpyBountyData> Bounties = [];

    [ViewVariables]
    public List<BlackMarketListing> BlackMarketListings = [];
}

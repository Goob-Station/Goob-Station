namespace Content.Goobstation.Shared.Spy;

[RegisterComponent]
public sealed partial class SpyBountyDatabaseComponent : Component
{
    [ViewVariables]
    public HashSet<SpyBountyData> Bounties = [];

    [ViewVariables]
    public HashSet<BlackMarketListing> BlackMarketListings = [];
}

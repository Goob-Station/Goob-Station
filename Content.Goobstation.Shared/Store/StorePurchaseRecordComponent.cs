namespace Content.Goobstation.Shared.Store;

/// <summary>
/// Tracks how many times a mind has purchased each store listing.
/// </summary>
[RegisterComponent]
public sealed partial class StorePurchaseRecordComponent : Component
{
    [DataField]
    public Dictionary<string, int> Purchases = new();
}

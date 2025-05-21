using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Storage;

/// <summary>
/// Simulates contents of the owner's StorageComponent interacting with whitelisted entity.
/// </summary>
[RegisterComponent]
public sealed partial class RelayInteractionStorageComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;
}

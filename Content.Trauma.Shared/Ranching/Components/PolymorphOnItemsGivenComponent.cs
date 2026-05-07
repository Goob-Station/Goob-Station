using Content.Shared.Whitelist;

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PolymorphOnItemsGivenComponent : Component
{
    [DataField]
    public EntityWhitelist Whitelist;

    [DataField]
    public List<EntProtoId> ReplacementEntities;

    [DataField]
    public int Amount;
}

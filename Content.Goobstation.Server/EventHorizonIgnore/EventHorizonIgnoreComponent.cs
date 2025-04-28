using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.EventHorizon;

[RegisterComponent]
public sealed partial class EventHorizonIgnoreComponent : Component
{
    [DataField]
    public EntityWhitelist HorizonWhitelist = new();
}

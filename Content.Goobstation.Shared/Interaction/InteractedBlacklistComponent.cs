using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Interaction;

// this is here since no reason to be in Content.Shared
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(InteractedBlacklistSystem))]
public sealed partial class InteractedBlacklistComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public EntityWhitelist? Blacklist;
}

using Content.Shared.Labels.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.HandRenamer;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedHandRenamerSystem))]
public sealed partial class HandRenamerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), Access(Other = AccessPermissions.ReadWriteExecute)]
    [DataField]
    public string AssignedName = string.Empty;

    [DataField]
    public EntityWhitelist Whitelist = new();

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public int MaxNameChars = 25;
}


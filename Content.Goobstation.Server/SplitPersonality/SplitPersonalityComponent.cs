using Robust.Shared.Containers;

namespace Content.Goobstation.Server.SplitPersonality;

[RegisterComponent]
public sealed partial class SplitPersonalityComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? OriginalMind;

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid?> GhostRoleDummies = [];

    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> MindRoles = [];

    [ViewVariables(VVAccess.ReadOnly)]
    public Container MindsContainer;

    [DataField]
    public int AdditionalMindsCount = 1;

}

[RegisterComponent]
public sealed partial class SplitPersonalityDummyComponent : Component
{
    public EntityUid? Host;
}

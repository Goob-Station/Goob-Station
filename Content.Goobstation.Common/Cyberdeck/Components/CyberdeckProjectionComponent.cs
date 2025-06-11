using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Cyberdeck.Components;

[RegisterComponent]
public sealed partial class CyberdeckProjectionComponent : Component
{
    [ViewVariables]
    public EntityUid? RemoteEntity;
}

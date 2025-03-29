using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Actions;

[RegisterComponent]
public sealed partial class ActionsProviderComponent : Component
{
    [DataField] public List<EntProtoId> Actions = new();
}

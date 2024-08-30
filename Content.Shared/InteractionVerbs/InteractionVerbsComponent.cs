using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.InteractionVerbs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InteractionVerbsComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<InteractionVerbPrototype>> AllowedVerbs = new();
}

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.InteractionVerbs;

/// <summary>
///     Specifies which verbs this entity may perform on its own, on any entity that the verb allows.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class OwnInteractionVerbsComponent : Component
{
    public override bool SendOnlyToOwner => true;

    [DataField, AutoNetworkedField]
    public List<ProtoId<InteractionVerbPrototype>> AllowedVerbs = new();

    // Too volatile to be worth networking; client and server just keep track of this field independently.
    [NonSerialized, ViewVariables]
    public Dictionary<(ProtoId<InteractionVerbPrototype>, EntityUid), TimeSpan> Cooldowns = new();
}

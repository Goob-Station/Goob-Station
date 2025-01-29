using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._NF.Interaction.Components;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState(true)]
// When an entity with this is removed from a hand, it is replaced with a placeholder entity that blocks the hand's use until re-equipped with the same prototype.
public sealed partial class HandPlaceholderComponent : Component
{
    /// <summary>
    /// A whitelist to match entities that this should accept.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    [DataField, AutoNetworkedField]
    public EntProtoId Prototype;
}


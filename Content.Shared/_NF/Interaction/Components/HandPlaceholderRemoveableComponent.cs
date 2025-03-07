using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._NF.Interaction.Components;

[RegisterComponent]
[NetworkedComponent]
// When an entity with this is removed from a hand, it is replaced with a placeholder entity that blocks the hand's use until re-equipped with the same prototype.
public sealed partial class HandPlaceholderRemoveableComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntProtoId? Prototype;
}

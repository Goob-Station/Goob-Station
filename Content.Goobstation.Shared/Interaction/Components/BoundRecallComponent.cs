using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Interaction.Components;

[RegisterComponent]
public sealed partial class BoundRecallComponent : Component
{
    [DataField]
    public EntityUid? BoundUser;

    // Action prototype used
    [DataField]
    public EntProtoId RecallAction = "ActionRecallBoundItem";
}

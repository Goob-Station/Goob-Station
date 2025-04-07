using Content.Goobstation.Common.MartialArts;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Server.Grab;

[RegisterComponent]
public sealed partial class GrabbingItemComponent : Component
{
    [DataField]
    public GrabStage GrabStageOverride = GrabStage.Hard;

    [DataField]
    public float EscapeAttemptModifier = 2f;
}

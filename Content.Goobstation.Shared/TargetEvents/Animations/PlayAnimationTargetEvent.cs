using Content.Goobstation.Common.TargetEvents;

namespace Content.Goobstation.Shared.TargetEvents.Animations;

[Serializable, DataDefinition]
public sealed partial class PlayAnimationTargetEvent : BaseTargetEvent
{
    [DataField]
    [AlwaysPushInheritance]
    public string AnimationID = "";
}

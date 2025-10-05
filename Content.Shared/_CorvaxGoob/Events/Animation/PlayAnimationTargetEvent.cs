namespace Content.Shared._CorvaxGoob.Events.Animation;

[Serializable, DataDefinition]
public sealed partial class PlayAnimationTargetEvent : BaseTargetEvent
{
    [DataField]
    [AlwaysPushInheritance]
    public string AnimationID = "";
}

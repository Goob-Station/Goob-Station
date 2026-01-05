namespace Content.Shared._CorvaxGoob.Events.StatusEffects;

[Serializable, DataDefinition]
public sealed partial class DoSparksTargetEvent : BaseTargetEvent
{
    [DataField]
    [AlwaysPushInheritance]
    public int MinSparks = 1;

    [DataField]
    [AlwaysPushInheritance]
    public int MaxSparks = 3;

    [DataField]
    [AlwaysPushInheritance]
    public int MinVelocity = 1;

    [DataField]
    [AlwaysPushInheritance]
    public int MaxVelocity = 4;

    [DataField]
    [AlwaysPushInheritance]
    public bool PlaySound = true;
}

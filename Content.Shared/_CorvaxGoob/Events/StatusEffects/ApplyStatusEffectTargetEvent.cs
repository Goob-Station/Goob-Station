namespace Content.Shared._CorvaxGoob.Events.StatusEffects;

[Serializable, DataDefinition]
public sealed partial class ApplyStatusEffectTargetEvent : BaseTargetEvent
{
    [DataField]
    [AlwaysPushInheritance]
    public string Key = "";

    [DataField]
    [AlwaysPushInheritance]
    public float Time = 0;

    [DataField]
    [AlwaysPushInheritance]
    public bool Refresh = true;

    [DataField]
    [AlwaysPushInheritance]
    public string ComponentType;
}

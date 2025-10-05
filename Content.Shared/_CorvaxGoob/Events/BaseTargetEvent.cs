namespace Content.Shared._CorvaxGoob.Events;

[ImplicitDataDefinitionForInheritors, DataDefinition]
public abstract partial class BaseTargetEvent : EntityEventArgs
{
    public EntityUid Target;
}

namespace Content.Goobstation.Common.TargetEvents;

[ImplicitDataDefinitionForInheritors, DataDefinition]
public abstract partial class BaseTargetEvent : EntityEventArgs
{
    public EntityUid Target;
}

namespace Content.Goobstation.Common.MisandryBox;

/// <summary>
/// Hook for marking-related things
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class MarkingSpecial
{
    public abstract void AfterEquip(EntityUid mob);
}

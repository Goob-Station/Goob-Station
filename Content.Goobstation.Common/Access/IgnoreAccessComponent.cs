namespace Content.Goobstation.Common.Access;

/// <summary>
/// If assigned to an entity with AccessComponent, will always allow access for some entities.
/// </summary>
[RegisterComponent]
public sealed partial class IgnoreAccessComponent : Component
{
    [ViewVariables]
    public HashSet<EntityUid> Ignore = new();
}

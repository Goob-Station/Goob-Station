using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Access;

/// <summary>
/// If assigned to an entity with AccessReaderComponent, will always allow access for some ignored entities.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IgnoreAccessComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public HashSet<EntityUid> Ignore = new();
}

using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// Filters entities that have MobStateComponent and a state that matches a configured list.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AutomationFilterSystem))]
[AutoGenerateComponentState]
public sealed partial class MobFilterComponent : Component
{
    /// <summary>
    /// Mob states allowed by the filter.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<MobState> States = new();
}

[Serializable, NetSerializable]
public enum MobFilterUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed partial class MobFilterToggleMessage(MobState state) : BoundUserInterfaceMessage
{
    public readonly MobState State = state;
}

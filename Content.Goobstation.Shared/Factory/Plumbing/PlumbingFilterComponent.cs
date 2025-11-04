using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Factory.Plumbing;

/// <summary>
/// Adds a liquid filter which can be changed via BUI.
/// Does nothing on its own, other systems have to check for it in their logic.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(PlumbingFilterSystem))]
[AutoGenerateComponentState]
public sealed partial class PlumbingFilterComponent : Component
{
    /// <summary>
    /// The reagent configured to be filtered.
    /// If this is set other reagents should be blocked by the machine.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<ReagentPrototype>? Filter;
}

[Serializable, NetSerializable]
public enum PlumbingFilterUiKey : byte
{
    Key
}

/// <summary>
/// Message sent to change a machine's filtered reagent.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlumbingFilterChangeMessage(ProtoId<ReagentPrototype>? filter) : BoundUserInterfaceMessage
{
    public readonly ProtoId<ReagentPrototype>? Filter = filter;
}

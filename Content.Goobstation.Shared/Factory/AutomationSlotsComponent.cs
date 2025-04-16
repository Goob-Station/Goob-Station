using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory;

/// <summary>
/// Adds slots to an entity that can be controlled by automation machines if it also has <see cref="AutomationComponent"/>.
/// Slots using <see cref="AutomationSlot"/> can provide or accept items.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AutomationSystem))]
public sealed partial class AutomationSlotsComponent : Component
{
    /// <summary>
    /// All input slots that can be automated.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<SinkPortPrototype>, AutomationSlot> Inputs = new();

    /// <summary>
    /// All output slots that can be automated.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<SourcePortPrototype>, AutomationSlot> Outputs = new();
}

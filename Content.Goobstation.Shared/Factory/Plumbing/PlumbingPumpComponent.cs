using Content.Goobstation.Shared.Factory.Slots;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Factory.Plumbing;

/// <summary>
/// Transfers liquid from an input machine's solution to an output machine's solution.
/// Basically a robotic arm for reagents.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(PlumbingPumpSystem), typeof(PlumbingLinkSystem))]
[AutoGenerateComponentPause]
public sealed partial class PlumbingPumpComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [ViewVariables]
    public bool ChainDirty = true;

    [ViewVariables]
    public readonly List<EntityUid> CachedProcessors = new();

    [ViewVariables]
    public EntityUid? CachedOutputMachine;

    [ViewVariables]
    public string? CachedOutputPort;

    [ViewVariables]
    public AutomationSlot? CachedOutputSlot;
}

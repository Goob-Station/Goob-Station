using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Factory.Plumbing;

/// <summary>
/// Transfers liquid from an input machine's solution to an output machine's solution.
/// Basically a robotic arm for reagents.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(PlumbingPumpSystem))]
[AutoGenerateComponentPause]
public sealed partial class PlumbingPumpComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);
}

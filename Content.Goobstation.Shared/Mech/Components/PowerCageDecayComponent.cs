using Content.Goobstation.Shared.Mech.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Handled by <see cref="MechPowerCageSystem"/>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class PowerCageDecayComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan DecayTime = TimeSpan.FromSeconds(30);

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;
}

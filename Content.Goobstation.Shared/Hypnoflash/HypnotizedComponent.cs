using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Hypnoflash;

/// <summary>
/// given to the entity that will add the first thing it hears into their objectives
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HypnotizedComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(8); // you have 8 seconds to hear something or else you just dont do anything

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan EndTime;
}

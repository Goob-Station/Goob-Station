using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Signifies that this entity is being blink-teleported to some spot.
/// TODO: cool shader for this fella
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MegafaunaActiveBlinkComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? BlinkTime;

    [ViewVariables, AutoNetworkedField]
    public EntityCoordinates Coordinates;

    [ViewVariables, AutoNetworkedField] // AutoNetworked intended here because it's spawning with a delay
    public SoundSpecifier? Sound;
}

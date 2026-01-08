using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.StationRadio.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VinylPlayerComponent : Component
{
    /// <summary>
    /// Audio entity that plays the sound near the structure.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SoundEntity;

    /// <summary>
    /// Server entity that relays all network payloads to station radio receivers.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ServerEntity;

    /// <summary>
    /// Signal port that is sending out music data.
    /// </summary>
    [DataField]
    public ProtoId<SourcePortPrototype> MusicOutputPort = "VynilMusic";

    /// <summary>
    /// If true, will only play music and send packages if the structure is powered.
    /// </summary>
    [DataField]
    public bool RequiresPower = true;
}

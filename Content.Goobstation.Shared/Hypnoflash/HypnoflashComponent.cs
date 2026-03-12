using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hypnoflash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HypnoflashComponent : Component
{
    [DataField]
    public SoundSpecifier ActivationSound = new SoundPathSpecifier("/Audio/Effects/Grenades/SelfDestruct/SDS_Charge2.ogg");

    [DataField] public float Radius = 4f;
    [DataField] public TimeSpan FullDuration = TimeSpan.FromSeconds(8);
    [DataField] public TimeSpan Duration = TimeSpan.FromSeconds(4.5); // also yes this is horrible
    [DataField] public EntProtoId? ProtoOnFlash; // will spawn the said entity after the spawn delay is at 0
    [DataField] public EntityWhitelist? Whitelist;
    [DataField] public EntityWhitelist? Blacklist;

    [DataField]
    public bool CheckEyeProt = true; // if you have eye protection, you wont be affected

    [DataField] // makes the thing unremoveable for the duration if true
    public bool Unremoveable;

    [DataField]
    public LocId PopupMessage = "hypnoflash-latch";

    [DataField]
    public object? Event;

    [DataField]
    public object? EventOnUser;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField]
    public TimeSpan? EndTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField]
    public TimeSpan? SpawnEndTime;

    [DataField, AutoNetworkedField]
    public EntityUid? Activator;
}

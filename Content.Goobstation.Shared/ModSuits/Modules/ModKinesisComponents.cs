using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits.Modules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModKinesisModuleComponent : Component
{
    [DataField]
    public float Range = 8f;

    [DataField]
    public float Gain = 4f;

    [DataField]
    public float MassSpeedFactor = 40f;

    [DataField]
    public float MinSpeed = 0.6f;

    [DataField]
    public float MaxSpeed = 6f;

    [DataField]
    public float ThrowThreshold = 1.5f;

    [DataField]
    public float GrabPower = 10f;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public SoundSpecifier HoldSound = new SoundPathSpecifier("/Audio/Weapons/weoweo.ogg", AudioParams.Default.WithLoop(true).WithVolume(-8f));

    [DataField]
    public SoundSpecifier ThrowSound = new SoundPathSpecifier("/Audio/Weapons/soup.ogg", AudioParams.Default.WithVolume(5f));

    [ViewVariables]
    public EntityUid? HoldStream;

    [DataField, AutoNetworkedField]
    public EntityUid? User;

    [DataField, AutoNetworkedField]
    public EntityUid? Held;

    [ViewVariables]
    public MapCoordinates? Cursor;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModKinesisUserComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Module;
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModKinesisHeldComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Module;
}

[Serializable, NetSerializable]
public sealed class ModKinesisGrabEvent : EntityEventArgs
{
    public NetEntity Target;
    public NetCoordinates Cursor;
}

[Serializable, NetSerializable]
public sealed class ModKinesisCursorEvent : EntityEventArgs
{
    public NetCoordinates Cursor;
}

[Serializable, NetSerializable]
public sealed class ModKinesisReleaseEvent : EntityEventArgs;

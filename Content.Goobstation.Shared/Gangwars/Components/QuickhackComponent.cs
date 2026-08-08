using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Opens unbolted doors ignoring access
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class QuickhackComponent : Component
{
    [DataField]
    public SoundSpecifier UseSound = new SoundCollectionSpecifier("Quickhack");

    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/_Goobstation/Gangs/quickhack_fail.ogg");

    [DataField]
    public TimeSpan ShootAnimation = TimeSpan.FromSeconds(1.12);

    [DataField, AutoNetworkedField]
    public QuickhackState State;

    [DataField, AutoNetworkedField]
    public TimeSpan? FiringResetAt;

    [DataField, AutoNetworkedField]
    public EntityUid? FiringTarget;
}

[Serializable, NetSerializable]
public enum QuickhackState : byte
{
    Idle,
    Firing,
}

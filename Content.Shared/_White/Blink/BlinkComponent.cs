using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Blink;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlinkComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Distance = 5f;

    [DataField, AutoNetworkedField]
    public bool IsActive = true;

    [DataField]
    public string BlinkDelay = "blink";

    [DataField, AutoNetworkedField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(0.1);

    [DataField, AutoNetworkedField]
    public float KnockdownRadius = 0.3f;

    [DataField]
    public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");
}

[Serializable, NetSerializable]
public sealed class BlinkEvent(NetEntity weapon, Vector2 direction) : EntityEventArgs
{
    public readonly NetEntity Weapon = weapon;
    public readonly Vector2 Direction = direction;
}

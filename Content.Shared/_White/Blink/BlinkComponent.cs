using System.Numerics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Blink;

[RegisterComponent, NetworkedComponent]
public sealed partial class BlinkComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Distance = 5f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float BlinkRate = 1f;

    public TimeSpan NextBlink;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg")
    {
        Params = AudioParams.Default.WithVolume(5f)
    };
}

[Serializable, NetSerializable]
public sealed class BlinkEvent(NetEntity weapon, Vector2 direction) : EntityEventArgs
{
    public readonly NetEntity Weapon = weapon;
    public readonly Vector2 Direction = direction;
}

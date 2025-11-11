using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.PowerToggle;

[NetworkedComponent]
public abstract partial class SharedTogglePowerComponent : Component
{
    [DataField("isTurnedOn")]
    public bool IsTurnedOn = true;

    [DataField("toggleInterval")]
    public TimeSpan ToggleInterval = TimeSpan.FromSeconds(2);

    [DataField("nextToggle")]
    public TimeSpan NextToggle = TimeSpan.Zero;
}

[Serializable, NetSerializable]
public sealed class TogglePowerMessage : EntityEventArgs
{
    public NetEntity User;
    public NetEntity Entity;

    public TogglePowerMessage(NetEntity entity, NetEntity user)
    {
        User = user;
        Entity = entity;
    }
}

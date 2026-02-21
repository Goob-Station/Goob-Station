using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._ES.PainFlash.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ESPainFlashComponent : Component;

[Serializable, NetSerializable]
public sealed class ESPainFlashMessage(FixedPoint2 damage, GameTick tick) : EntityEventArgs
{
    public FixedPoint2 Damage = damage;
    public GameTick Tick = tick;
}

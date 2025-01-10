using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._EinsteinEngines.Silicon.BlindHealing;

public abstract partial class SharedBlindHealingSystem : EntitySystem
{
    [Serializable, NetSerializable]
    protected sealed partial class HealingDoAfterEvent : SimpleDoAfterEvent
    {
    }
}

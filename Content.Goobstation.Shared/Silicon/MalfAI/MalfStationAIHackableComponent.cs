using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Silicon.MalfAI;

[RegisterComponent]
public sealed partial class MalfStationAIHackableComponent : Component
{
    [DataField]
    public bool Hacked = false;
}

[Serializable, NetSerializable]
public sealed partial class HackDoAfterEvent : SimpleDoAfterEvent;

[ByRefEvent]
public readonly struct OnHackedEvent(EntityUid hackerEnt, EntityUid hackedEnt)
{
    public readonly EntityUid HackerEntity = hackerEnt;

    public readonly EntityUid HackedEntity = hackedEnt;
};
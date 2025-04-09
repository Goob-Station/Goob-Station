using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Mimery;

[Serializable, NetSerializable]
public sealed partial class MimeryBookDoAfterEvent : SimpleDoAfterEvent
{

}

public sealed partial class FingerGunEvent : InstantActionEvent
{
}

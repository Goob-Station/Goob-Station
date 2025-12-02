using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Vehicles;

[Serializable, NetSerializable]
public enum ForkliftVisuals : byte
{
    CrateState,
}

[Serializable, NetSerializable]
public enum ForkliftCrateState : byte
{
    Empty,
    OneCrate,
    TwoCrates,
    ThreeCrates,
    FourCrates,
}

public sealed partial class ForkliftActionEvent : EntityTargetActionEvent;
public sealed partial class UnforkliftActionEvent : InstantActionEvent;

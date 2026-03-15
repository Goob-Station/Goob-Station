using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

[Serializable, NetSerializable]
public sealed partial class BindRecallDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class UnbindRecallDoAfterEvent : SimpleDoAfterEvent
{
}

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Medical.CPR;

[Serializable, NetSerializable]
public sealed partial class CPRDoAfterEvent : SimpleDoAfterEvent;

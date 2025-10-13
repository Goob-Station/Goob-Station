using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ModSuits;

[Serializable, NetSerializable]
public sealed partial class ToggleModPartsDoAfterEvent : SimpleDoAfterEvent;

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.BloodCult.Runes;

[Serializable, NetSerializable]
public sealed partial class RendingRuneDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum RendingRuneVisuals
{
    Active,
    Layer
}

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.WhiteDream.BloodCult.Runes;

[Serializable, NetSerializable]
public sealed partial class ApocalypseRuneDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public enum ApocalypseRuneVisuals
{
    Used,
    Layer
}

using Content.Shared.Guidebook;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Slasher.UI;

[Serializable, NetSerializable]
public enum SlasherKitSelectUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class SlasherKitSelectBoundUserInterfaceState(List<SlasherKitInfo> kits) : BoundUserInterfaceState
{
    public readonly List<SlasherKitInfo> Kits = kits;
}

[Serializable, NetSerializable]
public sealed class SlasherKitSelectedMessage(string kitId) : BoundUserInterfaceMessage
{
    public readonly string KitId = kitId;
}

[Serializable, NetSerializable]
public sealed record SlasherKitInfo(
    string Id,
    LocId Name,
    LocId? Description,
    SpriteSpecifier Sprite,
    SoundSpecifier? ThemeSong,
    string? AscensionId,
    string? RequiredAscension,
    bool Unlocked,
    ProtoId<GuideEntryPrototype>? Guide);

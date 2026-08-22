using Robust.Shared.Audio;
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
public sealed class SlasherKitSelectedMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}

[Serializable, NetSerializable]
public sealed class SlasherKitInfo(
    string id,
    string name,
    string description,
    SpriteSpecifier sprite,
    SoundSpecifier? themeSong,
    string? ascensionId,
    string? requiredAscension,
    bool unlocked,
    string? guide)
{
    public string Id = id;
    public string Name = name;
    public string Description = description;
    public SpriteSpecifier Sprite = sprite;
    public SoundSpecifier? ThemeSong = themeSong;

    public string? Guide = guide;
    public string? AscensionId = ascensionId;
    public string? RequiredAscension = requiredAscension;
    public bool Unlocked = unlocked;
}

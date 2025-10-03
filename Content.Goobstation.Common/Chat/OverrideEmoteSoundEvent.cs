using Robust.Shared.Audio;

namespace Content.Goobstation.Common.Chat;

[ByRefEvent]
public record struct OverrideEmoteSoundEvent(SoundSpecifier? Sound, AudioParams Params)
{
    public bool Cancelled = false;
};

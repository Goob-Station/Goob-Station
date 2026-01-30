using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.Chat;

[Serializable, NetSerializable]
public sealed class PlayGlobalSoundEvent : EntityEventArgs
{
    public SoundSpecifier SoundSpecifier { get; }

    public PlayGlobalSoundEvent(SoundSpecifier soundSpecifier)
    {
        this.SoundSpecifier = soundSpecifier;
    }
}

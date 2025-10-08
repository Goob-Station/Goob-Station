using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CyberSanity;

[Serializable, NetSerializable]
public sealed partial class StaticFlashEffectMessage : EntityEventArgs
{
    public float Duration = 2f;

    public StaticFlashEffectMessage(float duration)
    {
        Duration = duration;
    }
}

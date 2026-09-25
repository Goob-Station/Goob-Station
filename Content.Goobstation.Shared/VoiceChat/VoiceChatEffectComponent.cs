namespace Content.Goobstation.Shared.VoiceChat;

public enum VoiceEffect : byte
{
    None,
    Robot,
    Alien,
    Deep,
    High,
    Radio,
    Muffled,
    Abductor,
    Whisper,
}

[RegisterComponent]
public sealed partial class VoiceChatEffectComponent : Component
{
    [DataField]
    public VoiceEffect Effect = VoiceEffect.None;
}

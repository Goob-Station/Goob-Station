using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Wizard.Chuuni;

[Serializable, NetSerializable]
public sealed class CheckChuuniEyePatchEvent : EntityEventArgs
{
    public bool RequiresSpeech = false;
    public int Flags = 32 | 2;
    public int RequiredSlots = 2;
}

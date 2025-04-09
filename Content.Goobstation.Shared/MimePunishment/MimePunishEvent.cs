namespace Content.Goobstation.Shared.MimePunishment;

public sealed class MimePunishEvent : EntityEventArgs
{
    public float Chance;

    public MimePunishEvent(float chance)
    {
        Chance = chance;
    }
}

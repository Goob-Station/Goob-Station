namespace Content.Goobstation.Server.Cyberpsychosis;

[ByRefEvent]
public record struct GetCyberSanityModifiersEvent(int CurrentGain)
{
    public float GainModifier = 1;
}

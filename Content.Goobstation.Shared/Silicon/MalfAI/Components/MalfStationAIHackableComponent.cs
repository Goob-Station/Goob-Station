namespace Content.Goobstation.Shared.Silicon.MalfAI.Components;

[RegisterComponent]
public sealed partial class MalfStationAIHackableComponent : Component
{
    [DataField]
    public bool Hacked = false;

    [DataField]
    public TimeSpan SecondsToHack = TimeSpan.FromSeconds(10);
}
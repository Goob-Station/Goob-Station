using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Voidwalker.GameTicking.Rules;

[RegisterComponent]
public sealed partial class VoidwalkerRuleComponent : Component
{
    [DataField]
    public SoundPathSpecifier BriefingSound = new("/Audio/_Goobstation/Ambience/Antag/voidwalker_start.ogg");

    [DataField]
    public string VoidFaction = "VoidFaction";

    [DataField]
    public string NanotrasenFaction = "NanoTrasen";
}

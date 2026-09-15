using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Voidwalker.GameTicking.Rules;

// TODO : nuke this & replace with compass to station
[RegisterComponent]
public sealed partial class VoidwalkerRuleComponent : Component
{
    [DataField]
    public SoundPathSpecifier BriefingSound = new("/Audio/_Goobstation/Ambience/Antag/voidwalker_start.ogg");
}

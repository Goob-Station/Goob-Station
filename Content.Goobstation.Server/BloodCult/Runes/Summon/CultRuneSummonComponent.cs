using Robust.Shared.Audio;

namespace Content.Goobstation.Server.BloodCult.Runes.Summon;

[RegisterComponent]
public sealed partial class CultRuneSummonComponent : Component
{
    [DataField]
    public SoundPathSpecifier TeleportSound = new("/Audio/WhiteDream/BloodCult/veilin.ogg");
}

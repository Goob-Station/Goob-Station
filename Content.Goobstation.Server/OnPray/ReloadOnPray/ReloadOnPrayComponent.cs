using Robust.Shared.Audio;

namespace Content.Goobstation.Server.OnPray.ReloadOnPray;

[RegisterComponent]
public sealed partial class ReloadOnPrayComponent : Component
{
    [DataField]
    public SoundPathSpecifier ReloadSoundPath = new ("/Audio/Weapons/Guns/MagIn/shotgun_insert.ogg");
}

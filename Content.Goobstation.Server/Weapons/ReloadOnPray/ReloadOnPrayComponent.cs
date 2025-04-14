using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Weapons.ReloadOnPray;


[RegisterComponent]
public sealed partial class ReloadOnPrayComponent : Component
{
    [DataField]
    public SoundPathSpecifier ReloadSoundPath = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/shotgun_insert.ogg");
}

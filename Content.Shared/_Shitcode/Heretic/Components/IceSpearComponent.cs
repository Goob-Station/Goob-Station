using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class IceSpearComponent : Component
{
    [DataField]
    public EntityUid? ActionId;

    [DataField]
    public SoundSpecifier ShatterSound = new SoundCollectionSpecifier("GlassBreak");

    [DataField]
    public TimeSpan ShatterCooldown = TimeSpan.FromSeconds(45);
}

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarTouchComponent : Component
{
    [DataField]
    public EntityUid? StarTouchAction;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan DrowsinessTime = TimeSpan.FromSeconds(8);

    [DataField]
    public LocId Speech = "heretic-speech-star-touch";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Items/welder.ogg");

    [DataField]
    public SpriteSpecifier BeamSprite = new SpriteSpecifier.Rsi(new("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "cosmic_beam");

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(8);

    [DataField]
    public float CosmicFieldLifetime = 30f;
}

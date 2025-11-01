using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarTouchComponent : Component, ITouchSpell
{
    [DataField]
    public EntityUid? Action { get; set; }

    [DataField]
    public TimeSpan Cooldown { get; set; } = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan DrowsinessTime = TimeSpan.FromSeconds(8);

    [DataField]
    public LocId Speech { get; set; } = "heretic-speech-star-touch";

    [DataField]
    public SoundSpecifier? Sound { get; set; } = new SoundPathSpecifier("/Audio/Items/welder.ogg");

    [DataField]
    public SpriteSpecifier BeamSprite = new SpriteSpecifier.Rsi(new("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "cosmic_beam");

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(8);

    [DataField]
    public float CosmicFieldLifetime = 30f;
}

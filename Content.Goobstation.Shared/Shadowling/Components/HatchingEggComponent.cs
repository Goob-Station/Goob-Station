using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._EE.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class HatchingEggComponent : Component
{
    [DataField]
    public float CooldownTimer = 15.0f;

    [DataField]
    public EntityUid? ShadowlingInside;

    public bool HasBeenHatched;

    // I know this looks dumb but it looks more readable and less error-prone than when I tried with an array
    // maybe there's a better way but its only 3 messages that need to appear
    // Update: I found a way but im too lazy to do it so just deal with this
    public bool HasFirstMessageAppeared;
    public bool HasSecondMessageAppeared;
    public bool HasThirdMessageAppeared;

    public SoundSpecifier? CrackFirst = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/egg/crack01.ogg");
    public SoundSpecifier? CrackSecond = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/egg/crack02.ogg");
    public SoundSpecifier? CrackThird = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/egg/crack03.ogg");
}

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shadowling.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HatchingEggComponent : Component
{
    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan CooldownTimer = TimeSpan.FromSeconds(15);

    [DataField]
    public EntityUid? ShadowlingInside;

    [DataField]
    public bool HasBeenHatched;

    // I know this looks dumb but it looks more readable and less error-prone than when I tried with an array
    // maybe there's a better way but its only 3 messages that need to appear
    // Update: I found a way but im too lazy to do it so just deal with this
    [ViewVariables] public bool HasFirstMessageAppeared;
    [ViewVariables] public bool HasSecondMessageAppeared;
    [ViewVariables] public bool HasThirdMessageAppeared;

    [DataField] public SoundSpecifier? CrackFirst =
        new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/egg/crack01.ogg");
    [DataField] public SoundSpecifier? CrackSecond =
        new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/egg/crack02.ogg");
    [DataField] public SoundSpecifier? CrackThird =
        new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/egg/crack03.ogg");
}

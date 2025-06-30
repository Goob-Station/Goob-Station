using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shadowling.Components;


/// <summary>
/// This is used for the Ascension Egg
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscensionEggComponent : Component
{
    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(300);

    [DataField]
    public TimeSpan AscendingEffectInterval = TimeSpan.FromSeconds(8.02);

    [DataField]
    public EntityUid? Creator;

    [DataField]
    public bool StartTimer;

    [DataField]
    public string VerbName = "Start Ascension";

    [DataField]
    public EntityUid? ShadowlingInsideEntity;

    [DataField]
    public string? ShadowlingInside = "AscensionEggShadowlingInside";

    [DataField]
    public string? AscendingEffect = "ShadowlingAscendingEffect";

    [DataField]
    public bool AscendingEffectAdded;

    [DataField]
    public EntityUid? AscendingEffectEntity;

    [DataField]
    public SoundSpecifier? AscensionEnterSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/egg/ascension_enter.ogg");
}

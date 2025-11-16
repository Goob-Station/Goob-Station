using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components;


/// <summary>
/// This is used for the Ascension Egg
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingAscensionEggComponent : Component
{
    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    /// <summary>
    /// The Ascension Egg's cooldown timer. If it goes zero, the Shadowling ascends.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(300);

    /// <summary>
    /// This is used to determine when the effect, which happens right before the Shadowling ascends, appears.
    /// It is advised to not change this value, it is timed perfectly.
    /// </summary>
    [DataField]
    public TimeSpan AscendingEffectInterval = TimeSpan.FromSeconds(8.02);

    /// <summary>
    /// Marks the creator of the egg. Most likely the Shadowling.
    /// This helps prevent another Shadowling using another's Shadowling ascension egg to ascend.
    /// </summary>
    [DataField]
    public EntityUid? Creator;

    /// <summary>
    /// This is used to determine whether the ascension egg timer should be started.
    /// Used if the Shadowling pressed the verb to ascend.
    /// </summary>
    [DataField]
    public bool StartTimer;

    /// <summary>
    /// The verb text.
    /// </summary>
    [DataField]
    public string VerbName = "Start Ascension";

    /// <summary>
    /// This is an object that exists for visual reasons only.
    /// It is used to show the Shadowling's form on the egg.
    /// It might as well be considered an effect.
    /// </summary>
    [DataField]
    public EntityUid? ShadowlingInsideEntity;

    /// <summary>
    /// The effect prototype of ShadowlingInsideEntity above.
    /// </summary>
    [DataField]
    public EntProtoId ShadowlingInside = "AscensionEggShadowlingInside";

    /// <summary>
    /// The effect prototype of AscendingEffectInterval above.
    /// </summary>
    [DataField]
    public EntProtoId AscendingEffect = "ShadowlingAscendingEffect";

    /// <summary>
    /// Used to avoid duplicates of the ascending effect.
    /// </summary>
    [DataField]
    public bool AscendingEffectAdded;

    /// <summary>
    /// The entity that holds AscendingEffect above.
    /// </summary>
    [DataField]
    public EntityUid? AscendingEffectEntity;

    /// <summary>
    /// The sound that plays once the Shadowling enters the egg.
    /// </summary>
    [DataField]
    public SoundSpecifier? AscensionEnterSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/egg/ascension_enter.ogg");

    /// <summary>
    /// The theme that plays on successful ascension.
    /// </summary>
    [DataField]
    public SoundSpecifier AscensionTheme = new SoundPathSpecifier("/Audio/_Goobstation/Shadowling/shadowascension.ogg");
}

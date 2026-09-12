using Content.Shared._EinsteinEngines.Language;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._EinsteinEngines.Language.Components;

/// <summary>
/// Randomize language for the entity on given interval
/// </summary>
[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class RandomLanguageComponent : Component
{
    /// <summary>
    /// Whether or not this language is allowed.
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// Amount of time this language will be randomized.
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(300);

    /// <summary>
    /// The next update
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan Until;

    /// <summary>
    /// Original languages the entity spoke before randomization.
    /// </summary>
    [DataField]
    public List<ProtoId<LanguagePrototype>> OriginalSpoken = [];

    /// <summary>
    /// Original languages the entity understood before randomization
    /// </summary>
    [DataField]
    public List<ProtoId<LanguagePrototype>> OriginalUnderstood = [];
}

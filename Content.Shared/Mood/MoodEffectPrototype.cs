using Robust.Shared.Prototypes;

namespace Content.Shared.Mood;

[Prototype]
public sealed partial class MoodEffectPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    public string Description => Loc.GetString($"mood-effect-{ID}");

    /// <summary>
    /// If set, a new moodlet replaces the previous moodlet in the same category.
    /// </summary>
    [DataField, ValidatePrototypeId<MoodCategoryPrototype>]
    public string? Category;

    [DataField(required: true)]
    public float MoodChange;

    /// <summary>
    /// Duration in seconds. Zero means the moodlet stays until removed by another system.
    /// </summary>
    [DataField]
    public int Timeout;

    /// <summary>
    /// Hidden moodlets do not show popups or appear in the mood alert details.
    /// </summary>
    [DataField]
    public bool Hidden;

    /// <summary>
    /// Moodlet applied when this timed moodlet expires.
    /// </summary>
    [DataField]
    public ProtoId<MoodEffectPrototype>? MoodletOnEnd;
}

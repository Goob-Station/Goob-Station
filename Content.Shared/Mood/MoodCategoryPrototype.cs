using Robust.Shared.Prototypes;

namespace Content.Shared.Mood;

/// <summary>
/// A category where only one moodlet may be active at a time.
/// </summary>
[Prototype]
public sealed partial class MoodCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;
}

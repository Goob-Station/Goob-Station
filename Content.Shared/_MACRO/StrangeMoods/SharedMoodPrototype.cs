using Content.Shared.Dataset;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MACRO.StrangeMoods;

/// <summary>
///     Shared <see cref="StrangeMood"/> which multiple entities can have at once.
///     Non-shared moods can also be given to multiple entities at once, but
///     shared moods are GUARANTEED to be held by multiple entities.
/// </summary>
[Virtual, DataDefinition]
[Serializable, NetSerializable]
public partial class SharedMood
{
    /// <summary>
    /// The unique ID of the shared mood. This should NOT be assigned manually through YAML.
    /// Inherits from the ID of the <see cref="SharedMoodPrototype" />.
    /// </summary>
    [DataField]
    public string? UniqueId;

    /// <summary>
    /// The list of strange moods attached to the shared mood.
    /// </summary>
    [DataField]
    public List<StrangeMood> Moods = [];

    /// <summary>
    /// The dataset that the shared moods will pull from.
    /// </summary>
    [DataField]
    public ProtoId<DatasetPrototype>? Dataset;

    /// <summary>
    /// The amount of shared moods to be given.
    /// </summary>
    [DataField]
    public int Count = 1;
}

/// <summary>
///     Prototype for a <see cref="SharedMood"/>.
/// </summary>
[Prototype]
public sealed partial class SharedMoodPrototype : SharedMood, IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID
    {
        get => UniqueId ?? "";
        set => UniqueId = value;
    }
}
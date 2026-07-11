using Content.Shared.Dataset;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System.Linq;

namespace Content.Shared._MACRO.StrangeMoods;

/// <summary>
///     An individual 'mood' that will appear in an entity's StrangeMood UI.
/// </summary>
[Virtual, DataDefinition]
[Serializable, NetSerializable]
public partial class StrangeMood
{
    /// <summary>
    /// The prototype this mood was created from.
    /// Used for managing conflicts, this does not apply to admin-made moods.
    /// </summary>
    [DataField]
    public ProtoId<StrangeMoodPrototype>? ProtoId;

    /// <summary>
    /// A locale string of the mood name. Gets passed to
    /// <see cref="Loc.GetString"/> with <see cref="MoodVars"/>.
    /// </summary>
    [DataField(required: true)]
    public LocId MoodName;

    /// <summary>
    /// A locale string of the mood description. Gets passed to
    /// <see cref="Loc.GetString"/> with <see cref="MoodVars"/>.
    /// </summary>
    [DataField(required: true)]
    public LocId MoodDesc;

    /// <summary>
    /// A list of mood IDs that this mood will conflict with.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<StrangeMoodPrototype>> Conflicts = [];

    /// <summary>
    /// Additional localized words for the <see cref="MoodDesc"/>, for things like random
    /// verbs and nouns.
    /// Gets randomly picked from datasets in <see cref="StrangeMoodPrototype.MoodVarDatasets"/>.
    /// </summary>
    [DataField("rawMoodVars")]
    public Dictionary<string, string> MoodVars = new();

    public (string, object)[] GetLocArgs()
    {
        return MoodVars.Select(v => (v.Key, (object)v.Value)).ToArray();
    }

    public string GetLocName()
    {
        return Loc.GetString(MoodName, GetLocArgs());
    }

    public string GetLocDesc()
    {
        return Loc.GetString(MoodDesc, GetLocArgs());
    }
}

/// <summary>
///     Prototype for an individual 'mood' that will appear in an entity's StrangeMood UI.
/// </summary>
[Prototype]
public sealed partial class StrangeMoodPrototype : StrangeMood, IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID
    {
        get => ProtoId ?? "";
        set => ProtoId = value;
    }

    /// <summary>
    /// Extra mood variables that will be randomly chosen and provided
    /// for localizing <see cref="StrangeMood.MoodName"/> and <see cref="StrangeMood.MoodDesc"/>.
    /// </summary>
    [DataField("moodVars")]
    public Dictionary<string, ProtoId<DatasetPrototype>> MoodVarDatasets = new();

    /// <summary>
    /// If false, prevents the same variable from being rolled twice when rolling
    /// mood variables for this mood. Does not prevent the same mood variable
    /// from being present in other moods.
    /// </summary>
    [DataField]
    public bool AllowDuplicateMoodVars;
}
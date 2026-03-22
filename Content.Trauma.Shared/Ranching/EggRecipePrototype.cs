using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching;

/// <summary>
/// Prototype for ranching egg paths, chicken checks each prototype before it lays an egg and picks the first one that matches.
/// </summary>
[Prototype]
public sealed partial class EggRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField]
    public required EntProtoId Egg { get; set; }

    [DataField]
    public required EntProtoId RequiredChicken { get; set; }

    [DataField]
    public int HappinessRequired { get; set; }

    [DataField]
    public HashSet<ProtoId<TagPrototype>>? FoodTagsRequired { get; set; }

    [DataField]
    public List<Component>? ComponentsRequired { get; set; }

    [DataField]
    public bool NeedsSpecialFood { get; set; }
}

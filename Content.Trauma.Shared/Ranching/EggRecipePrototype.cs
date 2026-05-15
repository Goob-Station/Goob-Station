using Content.Shared.Chemistry.Reagent;
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

    /// <summary>
    /// The egg entity.
    /// </summary>
    [DataField]
    public required EntProtoId Egg { get; set; }

    /// <summary>
    /// Required chicken to lay the egg.
    /// </summary>
    [DataField]
    public required List<EntProtoId> RequiredChicken { get; set; }

    /// <summary>
    /// HappinessRequired, recipes are sorted by RequiresSpecialFood then happiness then weight.
    /// </summary>
    [DataField]
    public int HappinessRequired { get; set; } = 15;

    /// <summary>
    /// The "weight" the egg recipe has, used when sorting the recipes into a list should be used when you want an egg to have higher priority than another.
    /// </summary>
    [DataField]
    public int Weight { get; set; }

    /// <summary>
    /// What tags the food the chicken eats must have to lay the egg.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<TagPrototype>>? FoodTagsRequired { get; set; }

    /// <summary>
    /// The reagent required and its amount
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, float>? ReagentsRequired { get; set; }

    /// <summary>
    /// Chickens that don't need the FoodTagsRequired to lay the egg, used for chickens that lay the egg as default.
    /// </summary>
    [DataField]
    public required List<EntProtoId>? NoSpecialFoodRequiredChickens { get; set; }

    /// <summary>
    /// Used to set the amount of happiness that a chicken needs, e.g white hen needs 60 happiness to lay brown egg but brown hen only needs 15.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, int>? ChickensRequireDifferentHappiness { get; set; }

    /// <summary>
    /// Components required to lay the egg.
    /// </summary>
    [DataField]
    public ComponentRegistry? ComponentsRequired { get; set; }
}

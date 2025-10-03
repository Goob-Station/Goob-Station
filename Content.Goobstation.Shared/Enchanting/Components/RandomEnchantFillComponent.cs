using Content.Goobstation.Shared.Enchanting.Systems;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Gives an entity enchants by default.
/// </summary>
[RegisterComponent, Access(typeof(EnchantFillSystem))]
public sealed partial class RandomEnchantFillComponent : Component
{
    /// <summary>
    /// Dictionary of enchant ids and their data.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<EntProtoId<EnchantComponent>, RandomEnchantData> Enchants = new();

    [DataField]
    public MinMax MinMaxTier = new(1, 1);

    /// <summary>
    /// Whether this will apply real enchantments or fake ones (used for enchanted books)
    /// </summary>
    [DataField]
    public bool Fake;
}

[DataDefinition]
public sealed partial class RandomEnchantData
{
    [DataField]
    public MinMax MinMaxLevel = new(1, 1);

    [DataField]
    public float Weight = 1;
}

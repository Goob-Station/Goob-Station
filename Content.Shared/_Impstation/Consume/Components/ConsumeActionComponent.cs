using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Impstation.Consume.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConsumeActionComponent : Component
{
    [DataField]
    public EntityUid? ConsumeAction;

    [DataField]
    public string? ConsumeActionId;

    /// <summary>
    /// Damage dealt to target entity
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    [DataField]
    public bool CanEatRotten = true;

    /// <summary>
    /// Base consume speed. The quotient of the target and performers body mass is multiplied by this.
    /// </summary>
    [DataField]
    public float BaseConsumeSpeed = 10f;

    /// <summary>
    /// reagent ingested when eating.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> FoodReagentPrototype = "UncookedAnimalProteins";

    /// <summary>
    /// Percentage of toxin when eating a rotten corpse. Do not set a number less than 0 or more than 1
    /// </summary>
    [DataField]
    public float ToxinRatio = 0.5f;

    /// <summary>
    /// toxin ingested when eating a rotten corpse.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> Toxin = "GastroToxin";

    /// <summary>
    /// Solution Container to eat from! Yummy!
    /// </summary>
    [DataField]
    public string SolutionToDrinkFrom = "bloodstream";

    /// <summary>
    /// Body mass is multiplied by this to get the amount of
    /// food reagent you should get when eating a corpse.
    /// </summary>
    [DataField]
    public float MeatMultiplier = 0.25f;

    /// <summary>
    /// Percentage of Bloodstream to drink when consuming.
    /// </summary>
    [DataField]
    public float PortionDrunk = 0.1f;

    /// <summary>
    /// Percentage of how much we want to consume.
    /// </summary>
    [DataField]
    public float PercentageConsumed = 0.25f;

    [DataField]
    public string? PopupSelfStart;

    [DataField]
    public string? PopupOthersStart;

    [DataField]
    public string? PopupSelfEnd;

    [DataField]
    public string? PopupOthersEnd;

    [DataField]
    public bool CanGib = true;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? Blacklist;
}

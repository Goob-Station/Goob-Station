using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// Holds the data used for adding new abilities to the shadowling
/// </summary>
[DataDefinition]
public sealed partial class ShadowlingActionData
{
    [DataField]
    public int UnlockAtThralls;

    [DataField]
    public string ActionPrototype = string.Empty;

    [DataField]
    public string ActionComponentName = string.Empty;

    [DataField]
    public EntityUid ActionEntity;

    [ViewVariables]
    public bool Added = false;
}

/// <summary>
/// This is used for the Collective Mind ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingCollectiveMindComponent : Component
{
    [DataField]
    public EntProtoId ActionCollectiveMind = "ActionCollectiveMind";

    [DataField]
    public List<ShadowlingActionData> Locked = new()
    {
        new ShadowlingActionData
        {
            UnlockAtThralls = 3,
            ActionPrototype = "ActionSonicScreech",
            ActionComponentName = "ShadowlingSonicScreech"
        },
        new ShadowlingActionData
        {
            UnlockAtThralls = 10,
            ActionPrototype = "ActionBlindnessSmoke",
            ActionComponentName = "ShadowlingBlindnessSmoke"
        },
        new ShadowlingActionData
        {
            UnlockAtThralls = 5,
            ActionPrototype = "ActionNullCharge",
            ActionComponentName = "ShadowlingNullCharge"
        },
        new ShadowlingActionData
        {
            UnlockAtThralls = 7,
            ActionPrototype = "ActionBlackRecuperation",
            ActionComponentName = "ShadowlingBlackRecuperation"
        },
        new ShadowlingActionData
        {
            UnlockAtThralls = 12,
            ActionPrototype = "ActionEmpoweredEnthrall",
            ActionComponentName = "ShadowlingEmpoweredEnthrall"
        },
        new ShadowlingActionData()
        {
            UnlockAtThralls = 15,
            ActionPrototype = "ActionNoxImperii",
            ActionComponentName = "ShadowlingNoxImperii"
        },
        new ShadowlingActionData()
        {
            UnlockAtThralls = 20,
            ActionPrototype = "ActionAscendance",
            ActionComponentName = "ShadowlingAscendance"
        }
    };

    /// <summary>
    /// The abilities that have already been added from collective mind.
    /// </summary>
    [DataField]
    public int AbilitiesAdded;

    /// <summary>
    /// The amount of thralls that the Shadowling has, in order to check what abilities to give.
    /// </summary>
    [DataField]
    public int AmountOfThralls;

    /// <summary>
    /// The required thralls for ascension.
    /// Used to inform the Shadowling how many thralls they need to unlock the final ability.
    /// </summary>
    [DataField]
    public int ThrallsRequiredForAscension = 20;

    /// <summary>
    /// How long the Thralls get stunned once the Shadowling gains a new ability
    /// Gets modified with each new ability added.
    /// </summary>
    [DataField]
    public float BaseStunTime = 0.5f;

    /// <summary>
    /// The effect that is used once the ability activates.
    /// </summary>
    [DataField]
    public EntProtoId CollectiveMindEffect = "ShadowlingCollectiveMindEffect";
}

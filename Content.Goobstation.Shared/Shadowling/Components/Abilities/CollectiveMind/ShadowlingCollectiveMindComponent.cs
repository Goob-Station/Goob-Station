using Content.Shared._EE.Shadowling.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;


namespace Content.Shared._EE.Shadowling;
public sealed partial class ShadowlingActionData
{
    [DataField] public int UnlockAtThralls;
    [DataField] public string ActionPrototype = string.Empty;
    [DataField] public string ActionComponentName = string.Empty;
    [DataField] public EntityUid ActionEntity;
    public bool Added = false;
}

/// <summary>
/// This is used for the Collective Mind ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingCollectiveMindComponent : Component
{
    public string? ActionCollectiveMind = "ActionCollectiveMind";

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

    [DataField]
    public int AbilitiesAdded = 0;

    [DataField]
    public int AmountOfThralls;

    [DataField]
    public int ThrallsRequiredForAscension = 20;

    [DataField]
    public float BaseStunTime = 0.5f;

    [DataField]
    public string? CollectiveMindEffect = "ShadowlingCollectiveMindEffect";
}

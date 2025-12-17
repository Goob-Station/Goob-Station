using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// This is used for the Collective Mind ability.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShadowlingCollectiveMindComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionCollectiveMind";

    [DataField]
    public EntityUid? ActionEnt;

    [DataField, AutoNetworkedField]
    public List<ProtoId<ShadowlingAbilityUnlockPrototype>> AvailableAbilities = new();

    [DataField, AutoNetworkedField]
    public List<ProtoId<ShadowlingAbilityUnlockPrototype>> UnlockedAbilities = new();

    /// <summary>
    /// The amount of thralls that the Shadowling has, in order to check what abilities to give.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int AmountOfThralls;

    /// <summary>
    /// The required thralls for ascension.
    /// Used to inform the Shadowling how many thralls they need to unlock the final ability.
    /// </summary>
    [DataField, AutoNetworkedField]
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

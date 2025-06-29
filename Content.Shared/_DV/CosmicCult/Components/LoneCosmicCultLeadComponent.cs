using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Added to cult leaders that are the last ones standing to empower them. Fight back.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LoneCosmicCultLeadComponent : Component
{
    /// <summary>
    /// The amount of cultists that gets deducted from the required counts to perform spells.
    /// </summary>
    [DataField]
    public int CultAbilityDeduction = 1;
}

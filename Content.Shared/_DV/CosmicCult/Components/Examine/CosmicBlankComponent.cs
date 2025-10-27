using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components.Examine;

/// <summary>
/// Marker component for targets under the effect of Shunt Subjectivity or Astral Projection.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicBlankComponent : Component
{
    /// <summary>
    /// The status icon corresponding to the effect.
    /// </summary>
    [DataField]
    public ProtoId<SsdIconPrototype> StatusIcon = "CosmicSSDIcon";
}

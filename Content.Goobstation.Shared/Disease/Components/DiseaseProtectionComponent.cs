using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.Goobstation.Shared.Disease;

/// <summary>
/// Modifies strength of incoming and/or outgoing disease spread attempts for the entity or the wearer of the entity
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseProtectionComponent : Component
{
    [DataField]
    public DiseaseSpreadModifier Incoming = new();

    [DataField]
    public DiseaseSpreadModifier Outgoing = new();
}

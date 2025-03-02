using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.Collections.Generic;

namespace Content.Shared.Disease;

/// <summary>
/// Modifies strength of incoming and/or outgoing disease spread attempts for the entity or the wearer of the entity
/// </summary>
[RegisterComponent]
public sealed partial class DiseaseProtectionComponent : Component
{
    /// <summary>
    /// How much to modify incoming spread attempts' power.
    /// <summary>
    [DataField]
    public Dictionary<ProtoId<DiseaseSpreadPrototype>, float> Incoming = new();

    /// <summary>
    /// How much to modify outgoing spread attempts' power.
    /// <summary>
    [DataField]
    public Dictionary<ProtoId<DiseaseSpreadPrototype>, float> Outgoing = new();
}

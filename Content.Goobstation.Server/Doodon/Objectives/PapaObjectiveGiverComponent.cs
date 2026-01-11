using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Server.Doodons.Objectives;

[RegisterComponent]
public sealed partial class PapaObjectiveGiverComponent : Component
{
    /// <summary>
    /// Objective *entity prototype IDs* (like DevilContractObjective).
    /// </summary>
    [DataField] public List<string> Objectives = new();

    /// <summary>How many to pick from the pool.</summary>
    [DataField] public int Count = 2;
}

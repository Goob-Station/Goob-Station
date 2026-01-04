using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult;

/// <summary>
///     Marks an entity as a member of a blood cult. Also stores their powers.
/// </summary>
[RegisterComponent, NetworkedComponent, Virtual]
public partial class BloodCultistComponent : Component
{
    [DataField] public ProtoId<FactionIconPrototype> StatusIcon = "BloodCultist";
}

using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cult;

/// <summary>
///     Marks an entity as a member of a blood cult. Powers are stored in the server part.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultistComponent : Component
{
    [DataField] public ProtoId<FactionIconPrototype> StatusIcon = "BloodCultist";
}

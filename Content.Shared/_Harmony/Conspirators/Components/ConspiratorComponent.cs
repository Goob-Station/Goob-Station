using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Harmony.Conspirators.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConspiratorComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> ConspiratorIcon = "ConspiratorFaction";

    public override bool SessionSpecific => true;
}

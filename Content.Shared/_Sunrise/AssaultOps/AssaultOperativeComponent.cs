using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunrise.AssaultOps
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class AssaultOperativeComponent : Component
    {
        [DataField("statusIcon")]
        public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "SyndicateFaction";
    }
}

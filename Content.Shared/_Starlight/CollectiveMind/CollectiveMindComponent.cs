using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.CollectiveMind
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class CollectiveMindComponent : Component
    {
        [DataField("minds")]
        public Dictionary<string, int> Minds = new();

        [DataField]
        public ProtoId<CollectiveMindPrototype>? DefaultChannel = null;
    }
}

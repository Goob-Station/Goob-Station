using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.CollectiveMind
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class CollectiveMindComponent : Component
    {
        [DataField("minds")]
        public Dictionary<string, int> Minds = new();

        [DataField]
        public ProtoId<CollectiveMindPrototype>? DefaultChannel = null;

        [DataField]
        public bool HearAll = false;

        [DataField]
        public bool SeeAllNames = false;
    }
}

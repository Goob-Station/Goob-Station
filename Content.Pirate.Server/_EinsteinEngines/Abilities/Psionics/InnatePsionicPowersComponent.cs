using Content.Shared.Psionics;
using Robust.Shared.Prototypes;

namespace Content.Server.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class InnatePsionicPowersComponent : Component
    {
        /// <summary>
        ///     The list of all powers to be added on Startup
        /// </summary>
        [DataField]
        public List<ProtoId<PsionicPowerPrototype>> PowersToAdd = new();
    }
}

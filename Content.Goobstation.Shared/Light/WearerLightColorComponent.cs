using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Light
{
    [RegisterComponent]
    public sealed partial class WearerLightColorComponent : Component
    {
        /// <summary>
        /// Only wearers of this species tint the light; null means any humanoid
        /// </summary>
        [DataField]
        public ProtoId<SpeciesPrototype> Species = "Plasmamen";

        /// <summary>
        /// Fallback color, copied from the entity's PointLight at MapInit
        /// </summary>
        public Color DefaultColor;
    }
}

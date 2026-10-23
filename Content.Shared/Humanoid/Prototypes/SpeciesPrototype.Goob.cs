using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Prototypes;

public sealed partial class SpeciesPrototype
{
    [DataField]
    public SpriteSpecifier? InteractionHandSprite { get; private set; }
}

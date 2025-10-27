using Content.Server._DV.CosmicCult.Abilities;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent, Access(typeof(CosmicReturnSystem))]
public sealed partial class CosmicAstralBodyComponent : Component
{
    [DataField]
    public EntityUid OriginalBody;
}

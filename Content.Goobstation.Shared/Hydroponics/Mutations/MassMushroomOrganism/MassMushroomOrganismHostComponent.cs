using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hydroponics.Mutations.MassMushroomOrganism;

[RegisterComponent, NetworkedComponent]
public sealed partial class MassMushroomOrganismHostComponent : Component
{
    [DataField]
    public EntityUid AttachedMushroomOrganism;

    [DataField]
    public EntityUid? ActionFungalGrowth;
}

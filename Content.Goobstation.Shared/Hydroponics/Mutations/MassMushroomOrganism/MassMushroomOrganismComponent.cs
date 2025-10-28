using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hydroponics.Mutations.MassMushroomOrganism;

[RegisterComponent, NetworkedComponent]
public sealed partial class MassMushroomOrganismComponent : Component
{
    [DataField]
    public bool IsActive;

    [DataField]
    public EntityUid Host;
}

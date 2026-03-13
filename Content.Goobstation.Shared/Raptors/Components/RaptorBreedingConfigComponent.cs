using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Raptors.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RaptorBreedingConfigComponent : Component
{
    [DataField, AutoNetworkedField]
    public float StatMutationChance = 0.1f;

    [DataField, AutoNetworkedField]
    public float StatMutationRange = 0.1f;

    [DataField, AutoNetworkedField]
    public float ColorMutationChance = 0.05f;
}

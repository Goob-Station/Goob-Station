using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StarGazerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Summoner;
}

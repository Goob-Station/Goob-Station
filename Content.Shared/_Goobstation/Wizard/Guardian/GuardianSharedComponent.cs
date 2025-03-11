using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Guardian;

// I hate server components I hate server components I hate server components
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GuardianSharedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Host;
}

using Robust.Shared.GameStates;

namespace Content.Shared._CorvaxGoob.Weapons.Misc;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrapplingHookHunterComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Gun;

    [DataField, AutoNetworkedField]
    public EntityUid? Shooter;

    [DataField, AutoNetworkedField]
    public EntityUid? Target;
}

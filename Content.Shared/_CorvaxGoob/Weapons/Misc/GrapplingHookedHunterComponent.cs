using Robust.Shared.GameStates;

namespace Content.Shared._CorvaxGoob.Weapons.Misc;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrapplingHookedHunterComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Gun;

    [DataField, AutoNetworkedField]
    public EntityUid? Hook;
}

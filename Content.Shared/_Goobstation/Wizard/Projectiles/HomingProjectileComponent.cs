using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HomingProjectileComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid Target;

    [DataField]
    public float? HomingSpeed = 180f;

    [DataField]
    public Angle Tolerance = Angle.FromDegrees(1);
}

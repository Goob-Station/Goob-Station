using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Boomerang;

[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class BoomerangComponent : Component
{
    /// <summary>
    /// The entity that threw this boomerang
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Thrower;

    [DataField, AutoNetworkedField]
    public TimeSpan? TimeToReturn = TimeSpan.Zero;


    [DataField, AutoNetworkedField]
    public bool SendBack = false;
}

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Boomerang;

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

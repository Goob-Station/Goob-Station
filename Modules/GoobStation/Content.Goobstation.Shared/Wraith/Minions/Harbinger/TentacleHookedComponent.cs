using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class TentacleHookedComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// Every X seconds, throw the target towards us
    /// </summary>
    [DataField]
    public TimeSpan PerThrow = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Which entity to throw towards to.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ThrowTowards;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Projectile;

    [DataField]
    public float MaxDistance = 2f;

    [DataField]
    public float ThrowStrength = 10f;
}

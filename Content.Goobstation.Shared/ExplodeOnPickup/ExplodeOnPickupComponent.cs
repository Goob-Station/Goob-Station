using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ExplodeOnPickup;

/// <summary>
/// Whatever has this component will explode when it is picked up
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ExplodeOnPickupComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ExplosionIntensity = 300f;

    [DataField, AutoNetworkedField]
    public string ExplosionType = "HardBomb";

    [DataField, AutoNetworkedField]
    public int Slope = 5;

    [DataField, AutoNetworkedField]
    public int TileIntensity = 30;

    [DataField, AutoNetworkedField]
    public bool CreateVacuum;
}

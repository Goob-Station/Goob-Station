using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.ObraDinn;

[RegisterComponent,NetworkedComponent,AutoGenerateComponentState]
public sealed partial class ObraDinnClockComponent : Component
{
    /// <summary>
    /// how long holograms last
    /// </summary>
    [DataField]
    public float Lifetime = 30f;

    /// <summary>
    /// how close you need to be the  murder scene to activate
    /// </summary>
    [DataField]
    public float DistanceFromCrimeScene = 2f;

    /// <summary>
    /// For Internal cooldown
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.Zero;
    [DataField]
    public TimeSpan CooldownTime = TimeSpan.FromSeconds(1);

    /// <summary>
    /// data from the victim
    /// </summary>
    [DataField]
    public List<ObraDinnWitness> Witnesses = new List<ObraDinnWitness>();
    [DataField,AutoNetworkedField]
    public EntityCoordinates? Location = null;
    [DataField,AutoNetworkedField]
    public MapId? Map = null;
}

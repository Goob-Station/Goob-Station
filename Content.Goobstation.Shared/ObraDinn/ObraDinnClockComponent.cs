using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.ObraDinn;

[RegisterComponent,NetworkedComponent,AutoGenerateComponentState]
public sealed partial class ObraDinnClockComponent : Component
{

    [DataField]
    public float Lifetime = 30f;
    [DataField]
    public float DistanceFromCrimeScene = 2f;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.Zero;
    [DataField]
    public TimeSpan CooldownTime = TimeSpan.FromSeconds(1);

    [DataField]
    public List<ObraDinnWitness> Witnesses = new List<ObraDinnWitness>();
    [DataField,AutoNetworkedField]
    public EntityCoordinates? Location = null;
    [DataField,AutoNetworkedField]
    public MapId? Map = null;
}

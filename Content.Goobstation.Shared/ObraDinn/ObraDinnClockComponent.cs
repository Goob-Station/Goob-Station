using Robust.Shared.Map;

namespace Content.Goobstation.Shared.ObraDinn;

[RegisterComponent]
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
    [DataField]
    public EntityCoordinates? Location = null;
    [DataField]
    public MapId? Map = null;
}

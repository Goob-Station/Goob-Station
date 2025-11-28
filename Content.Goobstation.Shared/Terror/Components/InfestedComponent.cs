using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]

// Yes this is just a remake of timed spawner, it's easier this way, maybe
public sealed partial class InfestedComponent : Component
{
    /// <summary>
    /// Time before spawns get triggered.
    /// </summary>
    [DataField]
    public TimeSpan Timer = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Time before spawns get triggered.
    /// </summary>
    [DataField]
    public TimeSpan TimeToCure = TimeSpan.FromSeconds(300);

    /// Internal accumulators for timing.
     public TimeSpan Accumulator = TimeSpan.Zero;
     public TimeSpan CureAccumulator = TimeSpan.Zero;

    /// <summary>
    /// Number of spiders to spawn, decided randomly at runtime.
    /// </summary>
    [DataField]
    public int SpawnNumber;

    [DataField]
    public SoundSpecifier SpawnSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/hiss/lowHiss3.ogg");

    /// <summary>
    /// List of entities that can be spawned by an infested mob.
    /// </summary>
    [DataField]
    public List<EntProtoId> EggsTier1 = new()
{
    "SpiderlingRed",
    "SpiderlingGray",
    "SpiderlingGreen"
};
}

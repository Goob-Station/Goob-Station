using Content.Server._CorvaxGoob.Deathmatch_CS.Systems;
using Content.Shared.Storage;

namespace Content.Server._CorvaxGoob.Deathmatch_CS.Components;

[RegisterComponent, Access(typeof(CSRuleSystem))]
public sealed partial class CSRuleComponent : Component
{
    /// <summary>
    /// An entity spawned after a player is killed.
    /// </summary>
    [DataField("rewardSpawns")]
    public List<EntitySpawnEntry> RewardSpawns = new();

    /// <summary>
    /// The number of simultaneously active arenas.
    /// </summary>
    [DataField("numberOfSessions"), ViewVariables(VVAccess.ReadWrite)]
    public int NumberOfSessions = 2;

    /// <summary>
    /// Will the arenas being created be selected from the mapool or those that were selected earlier.
    /// </summary>
    [DataField("randomArena"), ViewVariables(VVAccess.ReadWrite)]
    public bool RandomArena = false;
}

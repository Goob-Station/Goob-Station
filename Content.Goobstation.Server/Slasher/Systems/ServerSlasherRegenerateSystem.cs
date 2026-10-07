using Content.Goobstation.Shared.Slasher;
using Content.Goobstation.Shared.Slasher.Components;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Server-side of the slasher Regenerate system.
/// </summary>
public sealed partial class ServerSlasherRegenerateSystem : EntitySystem
{
    [Dependency] private AntagLockerSpawnSystem _lockerSpawn = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherRegenerateComponent, SlasherRevivedFromDeathEvent>(OnRevivedFromDeath);
    }

    private void OnRevivedFromDeath(Entity<SlasherRegenerateComponent> ent, ref SlasherRevivedFromDeathEvent args)
    {
        _lockerSpawn.TryRelocateToLocker(ent.Owner);
    }
}

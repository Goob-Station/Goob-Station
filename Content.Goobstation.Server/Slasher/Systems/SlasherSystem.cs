using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Server-side rules for Slasher.
/// </summary>
public sealed class SlasherSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SlasherComponent, UpdateMobStateEvent>(OnUpdateMobState, after: [typeof(MobThresholdSystem)]);
    }

    private void OnStartup(Entity<SlasherComponent> ent, ref ComponentStartup args)
    {
        ApplyComponents(ent.Owner);
    }

    // This doesn't really get rid of the crit state but it does put the user into a sort of soft crit state due to server / client mismatch.
    // They can still move around and use skills they just can't attack. It also "blinds" their screen like normal crit.
    // I like the behavior so if it doesn't cause problems I'll keep it.
    private void OnUpdateMobState(Entity<SlasherComponent> ent, ref UpdateMobStateEvent args)
    {
        if (!_net.IsServer)
            return;

        if (args.State == MobState.Critical)
            args.State = MobState.Alive;
    }

    private void ApplyComponents(EntityUid uid)
    {
        EnsureComp<SlasherSummonMacheteComponent>(uid);
        EnsureComp<SlasherIncorporealComponent>(uid);
        EnsureComp<SlasherBloodTrailComponent>(uid);
        EnsureComp<SlasherPossessionComponent>(uid);
        EnsureComp<SlasherRegenerateComponent>(uid);
        EnsureComp<SlasherStaggerAreaComponent>(uid);
        EnsureComp<SlasherSoulStealComponent>(uid);
        EnsureComp<SlasherSummonMeatSpikeComponent>(uid);
    }
}

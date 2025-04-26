using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Objectives;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Spy;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SpySystem
{
    private static readonly SoundSpecifier StealSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Machines/wewewew.ogg");

    private static readonly SoundSpecifier StealSuccessSound = new SoundPathSpecifier("/Audio/Effects/kaching.ogg");
    private static readonly TimeSpan StealTime = TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    private void InitializeUplink()
    {
        SubscribeLocalEvent<SpyUplinkComponent, AfterInteractEvent>(OnInteractEvent);
        SubscribeLocalEvent<SpyUplinkComponent, SpyStealDoAfterEvent>(OnSpyAttemptSteal);
    }

    private void OnInteractEvent(Entity<SpyUplinkComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryGetSpyDatabaseEntity(out var nullableEnt) || nullableEnt is not { } dbEnt)
            return;

        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (!TryComp<StealTargetComponent>(target, out var stealComp))
            return;

        // Find matching bounty
        var matchingBounty = dbEnt.Comp.Bounties.FirstOrDefault(b =>
            b.StealGroup == stealComp.StealGroup &&
            !b.Claimed);

        if(!_protoMan.TryIndex<StealTargetGroupPrototype>(stealComp.StealGroup, out var prototype)) // store steal time here
            return;

        if (matchingBounty == null)
            return;

        var sound = _audio.PlayPvs(StealSound, ent, AudioParams.Default.WithLoop(true));
        if (sound is null)
            return;

        var doAfterArgs = new DoAfterArgs(_entityManager,
            args.User,
            StealTime,
            new SpyStealDoAfterEvent(GetNetEntity(sound.Value.Entity), stealComp.StealGroup), // Now passing steal group instead of entity
            ent,
            target: target);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnSpyAttemptSteal(Entity<SpyUplinkComponent> ent, ref SpyStealDoAfterEvent args)
    {
        _audio.Stop(GetEntity(args.Sound));

        if (args.Handled || args.Cancelled)
            return;

        if (!TrySetBountyClaimed(ent, args.Args.User, args.StealGroup, out var bountyData))
            return;

        if (args.Args.Target != null)
            QueueDel(args.Args.Target.Value);

        var reward = Spawn(bountyData.RewardListing.ProductEntity, Transform(args.Args.User).Coordinates);
        _hands.PickupOrDrop(args.Args.User, reward);
        _audio.PlayLocal(StealSuccessSound, ent, args.Args.User);
    }
}

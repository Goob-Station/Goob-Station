using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.Objectives.Components.Targets;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.Spy;

/// <summary>
/// This handles...
/// </summary>
public sealed partial class SpySystem
{
    /// <inheritdoc/>
    private void InitializeUplink()
    {
        SubscribeLocalEvent<SpyUplinkComponent, InteractUsingEvent>(OnInteractEvent);
        SubscribeLocalEvent<SpyUplinkComponent, SpyClaimBountyMessage>(OnSpyClaimBounty);
        SubscribeLocalEvent<SpyUplinkComponent, SpyStealDoAfterEvent>(OnSpyAttemptSteal);
    }

    private void OnSpyAttemptSteal(Entity<SpyUplinkComponent> ent, ref SpyStealDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target is null)
            return;

        // send item to blackmarket first.
        QueueDel(args.Args.Target);
        // do an effect and play a sound.
    }

    private void OnInteractEvent(Entity<SpyUplinkComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled
            || ent.Comp.ClaimedBounty is null
            || GetEntity(ent.Comp.ClaimedBounty.TargetEntity) != args.Target)
            return;

        var doAfterArgs = new DoAfterArgs(_entityManager,
            args.User,
            TimeSpan.FromSeconds(5),
            new SpyStealDoAfterEvent(),
            ent,
            target: args.Target);

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnSpyClaimBounty(Entity<SpyUplinkComponent> ent, ref SpyClaimBountyMessage args)
    {
        if (!TrySetBountyOwner(args.Bounty, ent)) // pass by ref so we can just modify it :troll:
            return;
        UpdateUserInterface(ent);
    }
}

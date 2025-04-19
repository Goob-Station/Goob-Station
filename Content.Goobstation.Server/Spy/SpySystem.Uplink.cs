using System.Linq;
using Content.Goobstation.Shared.Spy;
using Content.Server.Objectives.Components.Targets;
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
        //SubscribeLocalEvent<SpyUplinkComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SpyUplinkComponent, SpyClaimBountyMessage>(OnSpyClaimBounty);
    }

    private void OnSpyClaimBounty(Entity<SpyUplinkComponent> ent, ref SpyClaimBountyMessage args)
    {
        args.Bounty.Owner = GetNetEntity(ent);
    }
}

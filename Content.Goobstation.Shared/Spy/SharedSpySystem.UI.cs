using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Spy;

/// <summary>
/// This handles the serverside for the Spy uplink ui.
/// </summary>
public abstract partial class SharedSpySystem
{
    public void InitializeUI()
    {
        SubscribeLocalEvent<SpyUplinkComponent, SpyRequestUpdateInterfaceMessage>(OnRequestUpdate);
    }

    private void OnRequestUpdate(Entity<SpyUplinkComponent> ent, ref SpyRequestUpdateInterfaceMessage ev)
    {
        UpdateUserInterface(ent, user: ev.Actor);
    }

    /// <summary>
    /// Toggles the user interface for spies uplink
    /// </summary>
    /// <param name="user">The person who is opening the spy uplink ui.</param>
    /// <param name="uplink">The uplink entity itself</param>
    /// <param name="component">The uplink component being refreshed.</param>
    public void ToggleUi(EntityUid user, EntityUid uplink, SpyUplinkComponent? component = null)
    {
        if (!Resolve(uplink, ref component) || !_net.IsServer)
            return;

        if (!TryComp<ActorComponent>(user, out var actor)
            || !_ui.TryToggleUi(uplink, SpyUiKey.Key, actor.PlayerSession))
            return;

        UpdateUserInterface(uplink, user: user);
    }

    /// <summary>
    /// Updates the user interface for spies uplink
    /// </summary>
    /// <param name="uplink">The uplink entity itself</param>
    /// <param name="component">The uplink component being refreshed.</param>
    /// <param name="user">The person who is opening the spy uplink ui.</param>
    protected void UpdateUserInterface(EntityUid uplink, EntityUid? user = null, SpyUplinkComponent? component = null)
    {
        if (!Resolve(uplink, ref component) || !_net.IsServer)
            return;

        if (!TryGetSpyDatabaseEntity(out var dbNullEnt)
            || dbNullEnt is not {} dbEnt)
            return;
        var untilNextSkip = dbEnt.Comp.NextReset - _timing.CurTime;
        var state = new SpyUplinkUpdateState(dbEnt.Comp.Bounties, untilNextSkip);
        _ui.SetUiState(uplink, SpyUiKey.Key, state);
    }
}

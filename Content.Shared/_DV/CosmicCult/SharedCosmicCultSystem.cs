// SPDX-FileCopyrightText: 2025 Coenx-flex <coengmurray@gmail.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Antag;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared._DV.Roles;
using Content.Shared.AbilitySuppression;
using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared._DV.CosmicCult;

public abstract class SharedCosmicCultSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly MagicSuppressionSystem _suppression = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, ComponentGetStateAttemptEvent>(OnCosmicCultCompGetStateAttempt);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentGetStateAttemptEvent>(OnCosmicCultCompGetStateAttempt);
        SubscribeLocalEvent<CosmicCultComponent, ComponentStartup>(DirtyCosmicCultComps);
        SubscribeLocalEvent<CosmicCultLeadComponent, ComponentStartup>(DirtyCosmicCultComps);
    }

    public bool EntityIsCultist(EntityUid user)
    {
        if (!_mind.TryGetMind(user, out var mind, out _))
            return false;

        return HasComp<CosmicCultComponent>(user) || _role.MindHasRole<CosmicCultRoleComponent>(mind);
    }

    public bool EntitySeesCult(EntityUid user)
    {
        return EntityIsCultist(user) || HasComp<GhostComponent>(user);
    }

    /// <summary>
    /// Determines if a Cosmic Cult member can use their abilities
    /// </summary>
    public bool TryUseAbility(BaseActionEvent action)
    {
        if (action.Handled)
            return false;

        // Check if the magic is being suppressed
        if (_suppression.TryMagicSuppressed(action.Performer))
            return false;

        action.Handled = true;
        return true;
    }

    /// <summary>
    /// Determines if a Cosmic Cult Lead component should be sent to the client.
    /// </summary>
    private void OnCosmicCultCompGetStateAttempt(EntityUid uid, CosmicCultLeadComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    /// <summary>
    /// Determines if a Cosmic Cultist component should be sent to the client.
    /// </summary>
    private void OnCosmicCultCompGetStateAttempt(EntityUid uid, CosmicCultComponent comp, ref ComponentGetStateAttemptEvent args)
    {
        args.Cancelled = !CanGetState(args.Player);
    }

    /// <summary>
    /// The criteria that determine whether a Cult Member component should be sent to a client.
    /// </summary>
    /// <param name="player">The Player the component will be sent to.</param>
    private bool CanGetState(ICommonSession? player)
    {
        //Apparently this can be null in replays so I am just returning true.
        if (player?.AttachedEntity is not { } uid)
            return true;

        if (EntitySeesCult(uid)
            || HasComp<CosmicCultLeadComponent>(uid))
            return true;

        return HasComp<ShowAntagIconsComponent>(uid);
    }

    /// <summary>
    /// Dirties all the Cult components so they are sent to clients.
    ///
    /// We need to do this because if a Cult component was not earlier sent to a client and for example the client
    /// becomes a Cult then we need to send all the components to it. To my knowledge there is no way to do this on a
    /// per client basis so we are just dirtying all the components.
    /// </summary>
    private void DirtyCosmicCultComps<T>(EntityUid someUid, T someComp, ComponentStartup ev)
    {
        var cosmicCultComps = AllEntityQuery<CosmicCultComponent>();
        while (cosmicCultComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }

        var cosmicCultLeadComps = AllEntityQuery<CosmicCultLeadComponent>();
        while (cosmicCultLeadComps.MoveNext(out var uid, out var comp))
        {
            Dirty(uid, comp);
        }
    }
}

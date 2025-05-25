// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using System.Threading;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Maps;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

using Timer = Robust.Shared.Timing.Timer;

namespace Content.Client._DV.CosmicCult.Visuals;

/// <summary>
/// This handles rendering a preview of where the monument will be placed
/// </summary>
public sealed class MonumentPlacementPreviewSystem : EntitySystem
{
    //most of these aren't used by this system, see MonumentPlacementPreviewOverlay for a note on why they're here
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private MonumentPlacementPreviewOverlay? _cachedOverlay;
    private CancellationTokenSource? _cancellationTokenSource;

    private const int MinimumDistanceFromSpace = 3;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MonumentPlacementPreviewComponent, ActionAttemptEvent>(OnAttemptMonumentPlacement);
        SubscribeLocalEvent<CosmicCultLeadComponent, EventCosmicPlaceMonument>(OnCosmicPlaceMonument);
        SubscribeLocalEvent<CosmicCultLeadComponent, EventCosmicMoveMonument>(OnCosmicMoveMonument);
    }

    private void DoMonumentAnimation(EntityUid performer)
    {
        if (_cachedOverlay == null
            || _cancellationTokenSource == null
            || !VerifyPlacement(Transform(performer), out _))
            return;

        _cachedOverlay.LockPlacement = true;
        _cancellationTokenSource.Cancel(); //cancel the previous timer

        //remove the overlay automatically after the primeTime expires
        //no cancellation token for this one as this'll never need to get cancelled afaik
        Timer.Spawn(TimeSpan.FromSeconds(3.8), //anim takes 3.8s, might want to have the ghost disappear earlier but eh
            () =>
            {
                _overlay.RemoveOverlay<MonumentPlacementPreviewOverlay>();
                _cachedOverlay = null;
                _cancellationTokenSource = null;
            }
        );
    }

    private void OnCosmicMoveMonument(Entity<CosmicCultLeadComponent> ent, ref EventCosmicMoveMonument args) =>
        DoMonumentAnimation(args.Performer);

    private void OnCosmicPlaceMonument(Entity<CosmicCultLeadComponent> ent, ref EventCosmicPlaceMonument args) =>
        DoMonumentAnimation(args.Performer);

    //duplicated from the ability check, minus the station check because that can't be done clientside afaik?
    //and no popups because they're done in the ability check as well
    public bool VerifyPlacement(TransformComponent xform, out EntityCoordinates outPos)
    {
        outPos = new EntityCoordinates();

        //MAKE SURE WE'RE STANDING ON A GRID
        if (!TryComp(xform.GridUid, out MapGridComponent? grid))
            return false;

        //CHECK IF IT'S BEING PLACED CHEESILY CLOSE TO SPACE
        var worldPos = _transform.GetWorldPosition(xform); // this is technically wrong but basically fine

        foreach (var tile in _map.GetTilesIntersecting(xform.GridUid.Value, grid, new Circle(worldPos, MinimumDistanceFromSpace)))
            if (tile.IsSpace(_tileDef))
                return false;

        var localTile = _map.GetTileRef(xform.GridUid.Value, grid, xform.Coordinates);
        var targetIndices = localTile.GridIndices + new Vector2i(0, 1);
        var pos = _map.ToCenterCoordinates(xform.GridUid.Value, targetIndices, grid);
        outPos = pos;
        var box = new Box2(pos.Position + new Vector2(-1.4f, -0.4f), pos.Position + new Vector2(1.4f, 0.4f));

        //CHECK FOR ENTITY AND ENVIRONMENTAL INTERSECTIONS
        if (_lookup.AnyLocalEntitiesIntersecting(xform.GridUid.Value, box, LookupFlags.Dynamic | LookupFlags.Static, _player.LocalEntity))
            return false;

        //if all of those aren't false, return true
        return true;
    }

    private void OnAttemptMonumentPlacement(Entity<MonumentPlacementPreviewComponent> ent, ref ActionAttemptEvent args)
    {
        if (!TryComp<ConfirmableActionComponent>(ent, out var confirmableAction))
            return; //return if the action somehow doesn't have a confirmableAction comp

        //if we've already got a cached overlay, reset the timers & bump alpha back up to 1.
        //todo do that
        //should probably smoothly transition alpha back up to 1 but idrc (this will bother me a lot I'm lying) it's an incredibly specific thing that occurs in a .25s window at the end of a 10s wait
        //not a great solution but I'm not sure if a Real:tm: (also not entirely sure what a Real:tm: fix would be here tbh? hooking into ActionAttemptEvent?) fix would actually work here? need to investigate.
        if (_cachedOverlay != null && _cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            StartTimers(confirmableAction, _cancellationTokenSource, _cachedOverlay);

            if (_cachedOverlay.FadingOut) //if we're fading out
            {
                _cachedOverlay.FadingOut = false; //stop it

                var progress = (1 - (_cachedOverlay.FadeOutProgress / _cachedOverlay.FadeOutTime)) * _cachedOverlay.FadeInTime; //set fade in progress to 1 - fade out progress (so 70% out becomes 30% in)
                _cachedOverlay.FadeInProgress = progress;
                _cachedOverlay.FadingIn = true; //start fading in again
                _cachedOverlay.FadeOutProgress = 0; //stop the fadeout entirely
            } //no need for a special fade in case as well, that can go as normal

            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        //it's probably inefficient to make a new one every time, but this'll be happening like four times a round maybe
        //massive ctor because iocmanager hates me
        _cachedOverlay = new MonumentPlacementPreviewOverlay(EntityManager, _player, _proto, _timing, ent.Comp.Tier);
        _overlay.AddOverlay(_cachedOverlay);

        StartTimers(confirmableAction, _cancellationTokenSource, _cachedOverlay);
    }

    private void StartTimers(ConfirmableActionComponent comp, CancellationTokenSource tokenSource, MonumentPlacementPreviewOverlay overlay)
    {
        //remove the overlay automatically after the primeTime expires
        Timer.Spawn(comp.PrimeTime + comp.ConfirmDelay,
            () =>
            {
                _overlay.RemoveOverlay<MonumentPlacementPreviewOverlay>();
                _cachedOverlay = null;
                _cancellationTokenSource = null;
            },
            tokenSource.Token
        );

        //start a timer to start the fade out as well, with the same cancellation token
        Timer.Spawn(comp.PrimeTime + comp.ConfirmDelay - TimeSpan.FromSeconds(overlay.FadeOutTime),
            () =>
            {
                overlay.FadingOut = true;
            },
            tokenSource.Token
        );
    }
}

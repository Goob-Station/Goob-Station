
using Content.Client._Goobstation.Fishing.Overlays;
using Content.Client.DoAfter;
using Content.Shared._Goobstation.Fishing.Components;
using Content.Shared._Goobstation.Fishing.Events;
using Content.Shared._Goobstation.Fishing.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.GameStates;

namespace Content.Client._Goobstation.Fishing;

public sealed class FishingSystem : SharedFishingSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new FishingOverlay(EntityManager, _player));

        SubscribeLocalEvent<ActiveFishingSpotComponent, ComponentHandleState>(HandleComponentState);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<DoAfterOverlay>();
    }

    private void HandleComponentState(EntityUid uid, ActiveFishingSpotComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ActiveFishingSpotComponentState state)
            return;

        component.AttachedFishingLure = GetEntity(state.AttachedFishingLure);
        component.FishingStartTime = state.FishingStartTime;
        component.FishDifficulty = state.FishDifficulty;
        component.IsActive = state.IsActive;
    }
}

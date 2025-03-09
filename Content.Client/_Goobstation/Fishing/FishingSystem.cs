using Content.Client._Goobstation.Fishing.Overlays;
using Content.Client.DoAfter;
using Content.Shared._Goobstation.Fishing.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;

namespace Content.Client._Goobstation.Fishing;

public sealed class FishingSystem : SharedFishingSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new FishingOverlay(EntityManager, _player));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<DoAfterOverlay>();
    }
}

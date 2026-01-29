using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Client.Doodons;

public sealed class DoodonTownHallOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private DoodonTownHallOverlay? _overlay;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new DoodonTownHallOverlay(_entMan, _player);
        _overlayManager.AddOverlay(_overlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        if (_overlay != null)
            _overlayManager.RemoveOverlay(_overlay);
    }
}

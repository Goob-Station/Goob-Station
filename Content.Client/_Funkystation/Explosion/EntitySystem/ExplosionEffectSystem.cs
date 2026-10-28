using Content.Shared._Funkystation.Explosion.EntitySystems;
using Robust.Client.Graphics;

namespace Content.Client._Funkystation.Explosion.EntitySystem;

public sealed class ExplosionEffectSystem : SharedExplosionEffectSystem
{
    [Dependency] private IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new ExplosionShockWaveOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<ExplosionShockWaveOverlay>();
    }
}

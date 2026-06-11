using Content.Goobstation.Shared.Projectiles;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Projectiles;

public sealed class DodgeEffectVisualsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private DodgeEffectOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new DodgeEffectOverlay();
        _overlayMan.AddOverlay(_overlay);

        SubscribeLocalEvent<DodgeEffectComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<DodgeEffectComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnStartup(Entity<DodgeEffectComponent> ent, ref ComponentStartup args)
    {
        _overlay.AddEffect(ent.Owner, _timing.RealTime);
    }

    private void OnShutdown(Entity<DodgeEffectComponent> ent, ref ComponentShutdown args)
    {
        _overlay.RemoveEffect(ent.Owner);
    }
}

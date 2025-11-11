using Content.Goobstation.Client.Overlays;
using Content.Goobstation.Shared.CyberSanity;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Cyberpsychosis;

public sealed partial class CyberSanitySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private LowCyberSanityOverlay _sanityOverlay = new();
    private StaticFlashOverlay _flashOverlay = new();

    public override void Initialize()
    {
        base.Initialize();

        _flashOverlay.OnFinished = () => _overlay.RemoveOverlay(_flashOverlay);

        SubscribeLocalEvent<LowCyberSanityEffectComponent, ComponentInit>(OnSanityInit);
        SubscribeLocalEvent<LowCyberSanityEffectComponent, ComponentShutdown>(OnSanityShutdown);
        SubscribeLocalEvent<LowCyberSanityEffectComponent, LocalPlayerAttachedEvent>(OnSanityAttach);
        SubscribeLocalEvent<LowCyberSanityEffectComponent, LocalPlayerDetachedEvent>(OnSanityDetach);

        SubscribeLocalEvent<StaticFlashEffectMessage>(OnStaticFlash);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnStaticFlashDetach);
    }

    private void OnSanityInit(Entity<LowCyberSanityEffectComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity == ent.Owner)
            _overlay.AddOverlay(_sanityOverlay);
    }

    private void OnSanityShutdown(Entity<LowCyberSanityEffectComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == ent.Owner)
            _overlay.RemoveOverlay(_sanityOverlay);
    }

    private void OnSanityAttach(Entity<LowCyberSanityEffectComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _overlay.AddOverlay(_sanityOverlay);
    }

    private void OnSanityDetach(Entity<LowCyberSanityEffectComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlay.RemoveOverlay(_sanityOverlay);
    }

    private void OnStaticFlash(StaticFlashEffectMessage args)
    {
        _flashOverlay.Start = _timing.CurTime;
        _flashOverlay.End = _timing.CurTime + TimeSpan.FromSeconds(args.Duration);

        if (!_overlay.HasOverlay<StaticFlashOverlay>())
            _overlay.AddOverlay(_flashOverlay);
    }

    private void OnStaticFlashDetach(LocalPlayerDetachedEvent args)
    {
        if (_overlay.HasOverlay<StaticFlashOverlay>())
            _overlay.RemoveOverlay(_flashOverlay);
    }
}

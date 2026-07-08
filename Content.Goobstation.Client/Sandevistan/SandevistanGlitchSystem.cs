using Content.Goobstation.Shared.Sandevistan;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Trauma.Client.Sandevistan;

public sealed class SandevistanGlitchSystem : EntitySystem
{
    private SandevistanGlitchOverlay _proto = default!;

    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanGlitchComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanGlitchComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<SandevistanGlitchComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SandevistanGlitchComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _proto = new();
    }

    private void OnPlayerAttached(Entity<SandevistanGlitchComponent> ent, ref LocalPlayerAttachedEvent args) =>
        _overlayMan.AddOverlay(_proto);

    private void OnPlayerDetached(Entity<SandevistanGlitchComponent> ent, ref LocalPlayerDetachedEvent args) =>
        _overlayMan.RemoveOverlay(_proto);

    private void OnInit(Entity<SandevistanGlitchComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        _overlayMan.AddOverlay(_proto);
    }

    private void OnShutdown(Entity<SandevistanGlitchComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        _overlayMan.RemoveOverlay(_proto);
    }
}

using Content.Goobstation.Client.Slasher.Overlays;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Slasher.Systems;

public sealed class CharmedProgressOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private CharmedProgressOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherIdolComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SlasherIdolComponent, LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<SlasherIdolComponent, LocalPlayerDetachedEvent>(OnDetached);
        SubscribeLocalEvent<SlasherIdolComponent, ComponentShutdown>(OnShutdown);

        _overlay = new();
    }


    private void OnStartup(Entity<SlasherIdolComponent> ent, ref ComponentStartup args)
    {
        if (ent.Owner == _player.LocalEntity)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnAttached(Entity<SlasherIdolComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnDetached(Entity<SlasherIdolComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnShutdown(Entity<SlasherIdolComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            _overlayMan.RemoveOverlay(_overlay);
    }
}

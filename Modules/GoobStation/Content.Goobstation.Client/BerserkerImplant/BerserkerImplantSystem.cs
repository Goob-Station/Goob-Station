using Content.Goobstation.Shared.BerserkerImplant;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.BerserkerImplant;

public sealed class BerserkerImplantSystem : SharedBerserkerImplantSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private BerserkerImplantOverlay _overlay = new()
    {
        TintColor = new(255f, 47f, 0f),
        BlurAmount = 0.1f
    };

    public override void Initialize()
    {
        base.Initialize();

        _overlay.TintColor = Color.FromHex("#ff947cff");

        SubscribeLocalEvent<BerserkerImplantActiveComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<BerserkerImplantActiveComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<BerserkerImplantActiveComponent, LocalPlayerAttachedEvent>(OnAttach);
        SubscribeLocalEvent<BerserkerImplantActiveComponent, LocalPlayerDetachedEvent>(OnDetach);
    }

    private void OnInit(Entity<BerserkerImplantActiveComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        _overlayManager.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<BerserkerImplantActiveComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnAttach(Entity<BerserkerImplantActiveComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnDetach(Entity<BerserkerImplantActiveComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }
}

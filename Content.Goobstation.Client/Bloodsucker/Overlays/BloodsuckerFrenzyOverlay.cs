using Content.Goobstation.Shared.Bloodsuckers.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.Bloodsucker.Overlays;

public sealed class BloodsuckerFrenzyOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public BloodsuckerFrenzyOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var player = _player.LocalEntity;
        if (player == null)
            return;

        if (!_entity.HasComponent<BloodsuckerFrenzyOverlayComponent>(player))
            return;

        args.WorldHandle.DrawRect(args.WorldAABB, new Color(180, 0, 0, 80));
    }
}

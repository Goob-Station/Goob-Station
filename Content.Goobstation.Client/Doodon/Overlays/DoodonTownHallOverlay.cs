using Content.Goobstation.Shared.Doodons;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.Doodons;

public sealed class DoodonTownHallOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly IEntityManager _entMan;
    private readonly IPlayerManager _player;

    public DoodonTownHallOverlay(IEntityManager entMan, IPlayerManager player)
    {
        _entMan = entMan;
        _player = player;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not EntityUid local)
            return;

        if (!_entMan.TryGetComponent<DoodonTownHallVisionComponent>(local, out var vision) ||
            vision == null ||
            !vision.ShowTownHallRadius)
            return;

        var handle = args.WorldHandle;

        var query = _entMan.EntityQueryEnumerator<DoodonTownHallComponent, TransformComponent>();
        while (query.MoveNext(out _, out var hall, out var xform))
        {
            if (hall == null || xform == null)
                continue;

            handle.DrawCircle(
                xform.WorldPosition,
                hall.InfluenceRadius,
                Color.Cyan.WithAlpha(0.35f),
                filled: false);
        }
    }
}

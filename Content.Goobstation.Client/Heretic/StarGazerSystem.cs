using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Heretic;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Heretic;

public sealed class StarGazerSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (!_timing.IsFirstTimePredicted
        || !HasComp<StarGazeComponent>(_player.LocalEntity))
            return;

        var player = _player.LocalEntity.Value;
        var mousePos = _eye.PixelToMap(_input.MouseScreenPosition);

        if (mousePos.MapId == MapId.Nullspace)
            return;

        RaisePredictiveEvent(new LaserBeamEndpointPositionEvent(GetNetEntity(player), mousePos));
    }
}

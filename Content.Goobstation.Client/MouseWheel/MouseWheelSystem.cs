using System.Numerics;
using Content.Goobstation.Common.MouseWheel;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Client.Player;

namespace Content.Goobstation.Client.MouseWheel;

public sealed class MouseWheelSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MouseWheelUIEvent>(OnMouseWheel);
    }

    private void OnMouseWheel(ref MouseWheelUIEvent ev)
    {
        if (_player.LocalEntity is not { } player ||
            !TryComp(player, out ContentEyeComponent? eye))
            return;

        RaisePredictiveEvent(new SharedContentEyeSystem.RequestTargetZoomEvent
        {
            TargetZoom = eye.TargetZoom + new Vector2(ev.Delta * 0.1f) / SharedContentEyeSystem.ZoomMod,
        });
    }
}

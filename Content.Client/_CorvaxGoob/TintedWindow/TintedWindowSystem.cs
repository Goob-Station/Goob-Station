using Content.Shared._CorvaxGoob.TintedWindow;
using Robust.Client.Player;

namespace Content.Client._CorvaxGoob.TintedWindow;

public sealed class TintedWindowSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly OccluderSystem _occluder = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalEntity is null)
            return;

        if (TryComp<EyeComponent>(_player.LocalEntity, out var eye) && !eye.DrawFov)
            return;

        var query = EntityQueryEnumerator<TintedWindowComponent, OccluderComponent>();

        while (query.MoveNext(out var uid, out var window, out var occluder))
        {
            var angle = Angle.FromWorldVec(_xform.GetWorldPosition(_player.LocalEntity.Value) - _xform.GetWorldPosition(uid));
            var angleDelta = (_xform.GetWorldRotation(uid) - angle).Reduced().FlipPositive();
            var showOccluder = angleDelta < window.Arc / 2 || Math.Tau - angleDelta < window.Arc / 2;
            _occluder.SetEnabled(uid, showOccluder, occluder);
        }
    }
}

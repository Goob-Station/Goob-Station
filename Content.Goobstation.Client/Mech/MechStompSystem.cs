using System.Numerics;
using Content.Goobstation.Shared.Mech.Components;
using Content.Goobstation.Shared.Mech.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Mech;

public sealed class MechStompSystem : SharedMechStompSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private readonly Dictionary<EntityUid, Vector2> _lifted = new();

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<MechStompOverlayComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var stomp, out var sprite))
        {
            if (stomp.Landed || stomp.LocalStartTime is not { } start)
                continue;

            var progress = (float) ((_timing.RealTime - start) / stomp.SpriteLiftTiming);
            if (!_lifted.TryGetValue(uid, out var baseOffset))
            {
                baseOffset = sprite.Offset;
                _lifted[uid] = baseOffset;
            }

            if (progress >= 1f)
            {
                stomp.Landed = true;
                _lifted.Remove(uid);
                _sprite.SetOffset((uid, sprite), baseOffset);
                _audio.PlayStatic(stomp.SlamSound, Filter.Local(), stomp.Origin, true);
                continue;
            }

            var height = progress < 0.65f
                ? MathF.Sin(progress / 0.65f * MathF.PI / 2f)
                : 1f - MathF.Pow((progress - 0.65f) / 0.35f, 2f);
            _sprite.SetOffset((uid, sprite), baseOffset + new Vector2(0f, height * stomp.SpriteLiftHeight));
        }
    }
}

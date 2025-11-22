using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Fade;

public sealed class FadeVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FadeVisualsComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<FadeVisualsComponent> ent, ref ComponentInit args)
        => ent.Comp.AlphaChangeStart = _timing.CurTime;

    public override void FrameUpdate(float frameTime)
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<FadeVisualsComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var fade, out var sprite))
        {
            if (fade.AlphaChangeStart + fade.AlphaChangeTime < curTime)
            {
                RemCompDeferred(uid, fade);
                continue;
            }

            var elapsed = curTime - fade.AlphaChangeStart;
            var elapsedRatio = 1f - (float)Math.Min(1, elapsed.TotalSeconds / fade.AlphaChangeTime.TotalSeconds);

            _sprite.SetColor((uid, sprite), sprite.Color.WithAlpha(elapsedRatio));
        }
    }
}

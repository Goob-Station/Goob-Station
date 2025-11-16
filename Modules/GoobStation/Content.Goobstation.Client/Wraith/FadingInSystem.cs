using Robust.Client.GameObjects;
using Content.Goobstation.Shared.Wraith.Components;

namespace Content.Goobstation.Client.Wraith;

public sealed class FadingInSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprites = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FadingInComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, FadingInComponent fading, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        // Start fully transparent
        _sprites.SetColor((uid, sprite), sprite.Color.WithAlpha(0f));
        fading.Elapsed = 0f;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<FadingInComponent, SpriteComponent>();

        while (query.MoveNext(out var uid, out var fading, out var sprite))
        {
            if (fading.Finished)
                continue;

            fading.Elapsed += frameTime;

            var alpha = Math.Clamp(fading.Elapsed / fading.FadeInTime, 0f, 1f);
            _sprites.SetColor((uid, sprite), sprite.Color.WithAlpha(alpha));
        }
    }
}


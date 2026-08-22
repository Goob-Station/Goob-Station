using Content.Goobstation.Shared.Projectiles;
using Robust.Client.Graphics;
using Robust.Shared.Random;

namespace Content.Goobstation.Client.Projectiles;

public sealed class SpeedLinesEffectSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private SpeedLinesOverlay _overlay = default!;

    private const float SeedScale = 1000f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<AutoDodgeEffectEvent>(OnDodgeEffect);
        SubscribeLocalEvent<SpeedLinesEffectComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SpeedLinesEffectComponent, ComponentShutdown>(OnShutdown);

        _overlay = new();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<SpeedLinesEffectComponent>();
        while (query.MoveNext(out var uid, out var lines))
        {
            lines.Progress = Math.Min(1f, lines.Progress + frameTime / lines.Duration);
            if (lines.Progress >= 1f)
                RemCompDeferred<SpeedLinesEffectComponent>(uid);
        }
    }

    private void OnDodgeEffect(AutoDodgeEffectEvent ev)
    {
        if (!TryGetEntity(ev.Entity, out var uid))
            return;

        var lines = EnsureComp<SpeedLinesEffectComponent>(uid.Value);
        lines.Direction = ev.Direction;
        lines.Seed = _random.NextFloat() * SeedScale;
        lines.Progress = 0f;
    }

    private void OnStartup(Entity<SpeedLinesEffectComponent> ent, ref ComponentStartup args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<SpeedLinesEffectComponent> ent, ref ComponentShutdown args)
    {
        if (Count<SpeedLinesEffectComponent>() <= 1)
            _overlayMan.RemoveOverlay(_overlay);
    }
}

using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Shitcode.Heretic;

public sealed class AreaMansusGraspOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly SharedMansusGraspSystem _mansus;
    private readonly SharedTransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    private readonly ShaderInstance _shader;

    private static readonly ProtoId<ShaderPrototype> Shader = "EldritchSmoke";

    public AreaMansusGraspOverlay()
    {
        IoCManager.InjectDependencies(this);

        ZIndex = (int) Shared.DrawDepth.DrawDepth.FloorEffects;

        _mansus = _entMan.System<SharedMansusGraspSystem>();
        _transform = _entMan.System<SharedTransformSystem>();

        _shader = _prototype.Index(Shader).Instance();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;
        var bounds = args.WorldAABB.Enlarged(7f);
        var curTime = _timing.CurTime;

        handle.UseShader(_shader);
        var query = _entMan.EntityQueryEnumerator<AreaMansusGraspComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var grasp, out var xform))
        {
            if (grasp.ChannelStartTime is not { } startTime)
                continue;

            var pos = _transform.GetWorldPosition(xform);

            if (!bounds.Contains(pos))
                continue;

            var time = curTime - startTime;
            var range = _mansus.GetAreaGraspRange((uid, grasp), (float) time.TotalSeconds);
            handle.DrawCircle(pos, range, grasp.EffectColor);
        }

        var query2 = _entMan.EntityQueryEnumerator<AreaGraspEffectComponent, TransformComponent>();
        while (query2.MoveNext(out var uid, out var effect, out var xform))
        {
            if (effect.Size <= 0)
                continue;

            var time = (float) (curTime - effect.SpawnTime).TotalSeconds;
            var factor = time / effect.FadeTime;
            if (factor is < 0 or > 1)
                continue;

            var pos = _transform.GetWorldPosition(xform);

            if (!bounds.Contains(pos))
                continue;

            handle.DrawCircle(pos, effect.Size, effect.EffectColor.WithAlpha(1f - factor));
        }

        handle.UseShader(null);
    }
}

using System.Numerics;
using Content.Goobstation.Shared.Sandevistan;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Sandevistan;

/// <summary>
/// Plays the timestop swirl effect out of this entity for everyone who can see it.
/// </summary>
public sealed partial class SandevistanTimestopBurstOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SandevistanTimestopVision";

    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IGameTiming _timing = default!;

    private SharedTransformSystem? _transform;
    private SandevistanTimestopBurstSystem? _system;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _shader;

    public SandevistanTimestopBurstOverlay()
    {
        IoCManager.InjectDependencies(this);

        _shader = _proto.Index(Shader).InstanceUnique();
        ZIndex = 100;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (_transform == null && !_entMan.TrySystem(out _transform)
            || _system == null && !_entMan.TrySystem(out _system))
            return false;

        return args.Viewport.Eye != null;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var now = _timing.RealTime;
        foreach (var (uid, burst) in _system!.Bursts)
        {
            var progress = SandevistanTimestopBurstSystem.Progress(burst, now);
            if (progress <= 0f || progress >= 1f
                || !_entMan.TryGetComponent<TransformComponent>(uid, out var xform)
                || xform.MapID != args.MapId)
                continue;

            var handoff = burst.ReversedAt == null ? SmoothStep(0.85f, 1f, progress) : 0f;
            DrawBurst(in args, uid, progress, handoff);
        }
    }

    private void DrawBurst(in OverlayDrawArgs args, EntityUid user, float progress, float handoff)
    {
        var viewport = args.Viewport;
        var renderScale = viewport.RenderScale * viewport.Eye!.Scale;
        var center = viewport.WorldToLocal(_transform!.GetWorldPosition(user));
        center.Y = viewport.Size.Y - center.Y;
        var maxRadius = Vector2.Max(center, (Vector2) viewport.Size - center).Length() / renderScale.X;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture!);
        _shader.SetParameter("renderScale", renderScale);
        _shader.SetParameter("center", center);
        _shader.SetParameter("maxRadius", maxRadius);
        _shader.SetParameter("progress", progress);
        _shader.SetParameter("handoff", handoff);

        var handle = args.WorldHandle;
        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    private static float SmoothStep(float from, float to, float x)
    {
        var t = Math.Clamp((x - from) / (to - from), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
}

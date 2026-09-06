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
public sealed class SandevistanTimestopBurstOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SandevistanTimestopVision";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly SharedTransformSystem _transform;
    private readonly SandevistanTimestopBurstSystem _system;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _shader;

    public SandevistanTimestopBurstOverlay(SandevistanTimestopBurstSystem system)
    {
        IoCManager.InjectDependencies(this);

        _system = system;
        _transform = _entMan.System<SharedTransformSystem>();
        _shader = _proto.Index(Shader).InstanceUnique();
        ZIndex = 100;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args) =>
        args.Viewport.Eye != null;

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var query = _entMan.AllEntityQueryEnumerator<SandevistanTimestopBurstComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var burst, out var xform))
        {
            var elapsed = _timing.RealTime - burst.StartedAt;
            if (xform.MapID == args.MapId && elapsed < burst.Lasts)
                DrawBurst(in args, uid, elapsed);
        }

        if (_system.Reversing is { } user
            && _entMan.TryGetComponent<TransformComponent>(user, out var reversingXform)
            && reversingXform.MapID == args.MapId)
        {
            var elapsed = _timing.RealTime - _system.ReversedAt;
            if (elapsed < _system.ReversedLasts)
                DrawBurst(in args, user, _system.ReversedLasts - elapsed);
        }
    }

    private void DrawBurst(in OverlayDrawArgs args, EntityUid user, TimeSpan activated)
    {
        var viewport = args.Viewport;
        var renderScale = viewport.RenderScale * viewport.Eye!.Scale;
        var center = viewport.WorldToLocal(_transform.GetWorldPosition(user));
        center.Y = viewport.Size.Y - center.Y;
        var maxRadius = Vector2.Max(center, (Vector2) viewport.Size - center).Length() / renderScale.X;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture!);
        _shader.SetParameter("renderScale", renderScale);
        _shader.SetParameter("center", center);
        _shader.SetParameter("maxRadius", maxRadius);
        _shader.SetParameter("activated", (float) activated.TotalSeconds);

        var handle = args.WorldHandle;
        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}

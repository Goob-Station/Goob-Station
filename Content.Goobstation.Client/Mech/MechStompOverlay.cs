using System.Numerics;
using Content.Goobstation.Shared.Mech.Components;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Mech;

public sealed partial class MechStompOverlay : Overlay
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IGameTiming _timing = default!;

    private SharedTransformSystem? _xform;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly Dictionary<string, ShaderInstance> _shaders = new();

    public MechStompOverlay() => IoCManager.InjectDependencies(this);

    private ShaderInstance GetShader(string id)
    {
        if (!_shaders.TryGetValue(id, out var shader))
        {
            shader = _proto.Index<ShaderPrototype>(id).InstanceUnique();
            _shaders[id] = shader;
        }

        return shader;
    }

    private float GetElapsed(MechStompOverlayComponent stomp)
    {
        var elapsed = stomp.LocalStartTime is { } localStart
            ? _timing.RealTime - localStart
            : _timing.CurTime - (stomp.EndTime - stomp.Duration);

        return (float) (elapsed - stomp.SpriteLiftTiming).TotalSeconds;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null)
            return false;

        var query = _entMan.EntityQueryEnumerator<MechStompOverlayComponent>();
        while (query.MoveNext(out _, out var stomp))
        {
            if (GetElapsed(stomp) < stomp.Duration.TotalSeconds)
                return true;
        }

        return false;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null || args.Viewport.Eye == null)
            return;

        if (_xform is null && !_entMan.TrySystem(out _xform))
            return;

        var handle = args.WorldHandle;
        var renderScale = args.Viewport.RenderScale * args.Viewport.Eye.Scale;

        var query = _entMan.EntityQueryEnumerator<MechStompOverlayComponent>();
        while (query.MoveNext(out _, out var stomp))
        {
            var origin = _xform.ToMapCoordinates(stomp.Origin);
            if (origin.MapId != args.MapId)
                continue;

            var elapsed = GetElapsed(stomp);
            var duration = (float) (stomp.Duration - stomp.SpriteLiftTiming).TotalSeconds;
            if (elapsed is < 0f || elapsed >= duration)
                continue;

            var coords = args.Viewport.WorldToLocal(origin.Position);
            coords.Y = args.Viewport.Size.Y - coords.Y;

            var shader = GetShader(stomp.ShockwaveShader);
            shader.SetParameter("renderScale", renderScale);
            shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            shader.SetParameter("center", coords);
            shader.SetParameter("elapsed", elapsed);
            shader.SetParameter("maxRadius", stomp.DustRange * EyeManager.PixelsPerMeter);
            shader.SetParameter("dustColor", new Vector3(stomp.DustColor.R, stomp.DustColor.G, stomp.DustColor.B));

            var reach = stomp.DustRange * 1.6f + 1f;
            handle.UseShader(shader);
            handle.DrawRect(Box2.CenteredAround(origin.Position, new Vector2(reach * 2f)), Color.White);
            handle.UseShader(null);
        }
    }
}

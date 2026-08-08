using System.Numerics;
using Content.Goobstation.Shared.Slasher.Components;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Slasher.Overlays;

/// <summary>
/// Draws an expanding shockwave ring.
/// </summary>
public sealed class SlasherStaggerOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private SharedTransformSystem? _xform;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private readonly Dictionary<string, ShaderInstance> _shaders = new();

    public SlasherStaggerOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    private ShaderInstance GetShader(string id)
    {
        if (!_shaders.TryGetValue(id, out var shader))
        {
            shader = _proto.Index<ShaderPrototype>(id).InstanceUnique();
            _shaders[id] = shader;
        }

        return shader;
    }

    private float GetProgress(SlasherStaggerOverlayComponent wave)
    {
        var start = wave.EndTime - wave.Duration;
        return (float) ((_timing.CurTime - start).TotalSeconds / wave.Duration.TotalSeconds);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (args.Viewport.Eye == null)
            return false;

        var query = _entMan.AllEntityQueryEnumerator<SlasherStaggerOverlayComponent>();
        while (query.MoveNext(out _, out var wave))
        {
            if (GetProgress(wave) < 1f)
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

        var query = _entMan.AllEntityQueryEnumerator<SlasherStaggerOverlayComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var wave, out var xform))
        {
            if (xform.MapID != args.MapId)
                continue;

            var progress = GetProgress(wave);
            if (progress is < 0f or >= 1f)
                continue;

            var worldPos = _xform.GetWorldPosition(uid);

            var coords = args.Viewport.WorldToLocal(worldPos);
            coords.Y = args.Viewport.Size.Y - coords.Y;

            var maxRadius = wave.Range * EyeManager.PixelsPerMeter;

            var color = wave.RingColor;
            var shader = GetShader(wave.ShockwaveShader);
            shader.SetParameter("renderScale", renderScale);
            shader.SetParameter("ringColor", new Vector3(color.R, color.G, color.B));
            shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
            shader.SetParameter("center", coords);
            shader.SetParameter("progress", progress);
            shader.SetParameter("maxRadius", maxRadius);

            handle.UseShader(shader);
            handle.DrawRect(args.WorldBounds, Color.White);
            handle.UseShader(null);
        }
    }
}

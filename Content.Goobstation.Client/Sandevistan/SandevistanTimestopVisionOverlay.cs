using System.Numerics;
using Content.Goobstation.Shared.Sandevistan;
using DrawDepthEnum = Content.Shared.DrawDepth.DrawDepth;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Sandevistan;

/// <summary>
/// This is the grey look the background gets during a sandevistan timestop.
/// Ignores player / item entities.
/// </summary>
public sealed class SandevistanTimestopVisionOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> WorldShader = "SandevistanTimestopWorld";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities | OverlaySpace.BeforeLighting;

    private readonly ShaderInstance _world;

    public SandevistanTimestopVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _world = _proto.Index(WorldShader).InstanceUnique();

        ZIndex = (int) DrawDepthEnum.Objects;
    }


    protected override bool BeforeDraw(in OverlayDrawArgs args) =>
        args.Viewport.Eye != null;

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } player
            || !_entMan.TryGetComponent<SandevistanTimestopVisionComponent>(player, out var vision))
            return;

        if (args.Space == OverlaySpace.BeforeLighting)
            DrawLighting(in args, vision);
        else
            DrawWorld(in args, vision);
    }

    private void DrawWorld(in OverlayDrawArgs args, SandevistanTimestopVisionComponent vision)
    {
        if (ScreenTexture is null)
            return;

        _world.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _world.SetParameter("activated", (float) (_timing.RealTime - vision.StartedAt).TotalSeconds);

        var handle = args.WorldHandle;
        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(_world);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    private void DrawLighting(in OverlayDrawArgs args, SandevistanTimestopVisionComponent vision)
    {
        var viewport = args.Viewport;
        var elapsed = (float) (_timing.RealTime - vision.StartedAt).TotalSeconds;
        var fadeIn = (float) vision.FadeIn.TotalSeconds;
        var settled = Math.Clamp((elapsed - 0.43f) / (fadeIn - 0.43f), 0f, 1f);
        var color = vision.LightColor.WithAlpha(vision.LightColor.A * settled);
        var worldHandle = args.WorldHandle;
        var bounds = args.WorldBounds;
        var lightScale = viewport.LightRenderTarget.Size / (Vector2) viewport.Size;
        var newScale = viewport.RenderScale / (Vector2.One / lightScale);
        var localMatrix = viewport.LightRenderTarget.GetWorldToLocalMatrix(viewport.Eye!, newScale);

        worldHandle.RenderInRenderTarget(viewport.LightRenderTarget,
            () =>
            {
                worldHandle.SetTransform(localMatrix);
                worldHandle.DrawRect(bounds, color);
                worldHandle.SetTransform(Matrix3x2.Identity);
            },
            null);
    }
}

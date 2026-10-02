using System.Numerics;
using Content.Goobstation.Shared.Sandevistan;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Sandevistan;

/// <summary>
/// This is the green look the world gets during sandevistan time dilation.
/// </summary>
public sealed class SandevistanSlowdownVisionOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SandevistanSlowdownVision";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace | OverlaySpace.BeforeLighting;
    private readonly ShaderInstance _shader;

    public SandevistanSlowdownVisionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _shader = _proto.Index(Shader).InstanceUnique();

        ZIndex = 100;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args) =>
        args.Viewport.Eye != null;

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity is not { } player
            || !_entMan.TryGetComponent<SandevistanSlowdownVisionComponent>(player, out var vision))
            return;

        if (args.Space == OverlaySpace.BeforeLighting)
            DrawLighting(in args, vision);
        else
            DrawSpeed(in args);
    }

    private void DrawSpeed(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null)
            return;

        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var handle = args.WorldHandle;
        handle.SetTransform(Matrix3x2.Identity);
        handle.UseShader(_shader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }

    private void DrawLighting(in OverlayDrawArgs args, SandevistanSlowdownVisionComponent vision)
    {
        var viewport = args.Viewport;
        var worldHandle = args.WorldHandle;
        var bounds = args.WorldBounds;
        var lightScale = viewport.LightRenderTarget.Size / (Vector2) viewport.Size;
        var newScale = viewport.RenderScale / (Vector2.One / lightScale);
        var localMatrix = viewport.LightRenderTarget.GetWorldToLocalMatrix(viewport.Eye!, newScale);

        worldHandle.RenderInRenderTarget(viewport.LightRenderTarget,
            () =>
            {
                worldHandle.SetTransform(localMatrix);
                worldHandle.DrawRect(bounds, vision.LightColor);
                worldHandle.SetTransform(Matrix3x2.Identity);
            },
            null);
    }
}

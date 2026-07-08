using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Client.Sandevistan;

public sealed class SandevistanGlitchOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> Shader = "SandevistanGlitch";

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;
    private readonly ShaderInstance _glitchShader;

    private float _intensity = 1f;

    public SandevistanGlitchOverlay()
    {
        IoCManager.InjectDependencies(this);
        _glitchShader = _proto.Index(Shader).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entMan.TryGetComponent(_player.LocalEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        return true;
    }

    public void SetIntensity(float intensity) =>
        _intensity = intensity;

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _glitchShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _glitchShader.SetParameter("intensity", _intensity);
        handle.UseShader(_glitchShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}

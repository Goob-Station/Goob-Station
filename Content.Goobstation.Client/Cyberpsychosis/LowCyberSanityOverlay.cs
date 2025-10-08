using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Overlays;

public sealed class LowCyberSanityOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private ShaderInstance _shader;
    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public LowCyberSanityOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = 102;

        _shader = _proto.Index<ShaderPrototype>("AdvancedStatic").InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        _shader.SetParameter("mixAmount", 0.3f);
        _shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

        var handle = args.WorldHandle;
        var viewport = args.WorldBounds;

        handle.UseShader(_shader);
        handle.DrawRect(viewport, Color.FromHex("#ffd5ce8c"));
        handle.UseShader(null);
    }
}

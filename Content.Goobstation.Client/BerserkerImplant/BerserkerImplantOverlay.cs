using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Matrix3x2 = System.Numerics.Matrix3x2;
using Vector3 = Robust.Shared.Maths.Vector3;

namespace Content.Goobstation.Client.BerserkerImplant;

public sealed class BerserkerImplantOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _blurShader;

    public Color TintColor = new();

    public float BlurAmount = 0f;

    public BerserkerImplantOverlay()
    {
        IoCManager.InjectDependencies(this);

        _blurShader = _prototype.Index<ShaderPrototype>("BlurryVisionX").InstanceUnique();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var worldHandle = args.WorldHandle;
        var worldBounds = args.WorldBounds;

        _blurShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _blurShader.SetParameter("BLUR_AMOUNT", BlurAmount);

        worldHandle.UseShader(_blurShader);
        worldHandle.DrawRect(worldBounds, TintColor);
        worldHandle.UseShader(null);
    }
}

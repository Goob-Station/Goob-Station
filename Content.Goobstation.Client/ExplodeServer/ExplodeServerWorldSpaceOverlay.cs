using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.ExplodeServer;

public sealed class ExplodeServerWorldSpaceOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _blurShader;
    public Color TintColor = new();
    
    public bool IsActive = true;
    
    public float BlurAmount = 0f;

    public ExplodeServerWorldSpaceOverlay()
    {
        ZIndex = 1;
        var cache = IoCManager.Resolve<IResourceCache>();
        IoCManager.InjectDependencies(this);
        _blurShader = _prototype.Index<ShaderPrototype>("BlurryVisionX").InstanceUnique();
    }
    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null || !IsActive)
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
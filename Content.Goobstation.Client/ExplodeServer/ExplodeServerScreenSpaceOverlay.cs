using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.ExplodeServer;

public sealed class ExplodeServerScreenSpaceOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;
    public bool IsActive = false;

    private readonly Font _font;

    public ExplodeServerScreenSpaceOverlay()
    {
        ZIndex = 201;
        var cache = IoCManager.Resolve<IResourceCache>();
        IoCManager.InjectDependencies(this);
        _font = new VectorFont(cache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 36);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!IsActive)
            return;
        const string text = "SERVER EXPLOSION IMMINENT";
        var screenHandle = args.ScreenHandle;
        var size = screenHandle.GetDimensions(_font, text, 1f);
        var center = args.ViewportBounds.Size / 2f;
        var pos = center - size / 2f;
        screenHandle.DrawString(_font, pos, text, Color.White);
    }
}
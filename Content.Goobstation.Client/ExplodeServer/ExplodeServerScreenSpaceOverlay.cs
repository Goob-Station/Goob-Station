using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;

namespace Content.Goobstation.Client.ExplodeServer;

public sealed class ExplodeServerScreenSpaceOverlay : Overlay
{
    [Dependency] private readonly IClyde _clyde = default!;
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;
    public bool IsActive = false;

    private Font _font;

    public ExplodeServerScreenSpaceOverlay()
    {
        ZIndex = 201;
        var cache = IoCManager.Resolve<IResourceCache>();
        IoCManager.InjectDependencies(this);
        _font = new VectorFont(cache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 15);
    }
    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!IsActive)
            return;
        const string text = "SERVER IS GONNA EXPLODE!";
        var screenHandle = args.ScreenHandle;
        var center = _clyde.ScreenSize / 2;
        screenHandle.DrawString(_font,center, text, Color.White);
    }
}
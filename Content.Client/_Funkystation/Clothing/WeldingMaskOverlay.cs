using Content.Client.Resources;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;

namespace Content.Client._Funkystation.Clothing;

public sealed class WeldingMaskOverlay(IResourceCache cache) : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public string? CurrentTexturePath;

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (string.IsNullOrEmpty(CurrentTexturePath))
            return;

        var texture = cache.GetTexture(CurrentTexturePath);
        var handle = args.WorldHandle;

        handle.DrawTextureRect(texture, args.WorldBounds, Color.White);
    }
}

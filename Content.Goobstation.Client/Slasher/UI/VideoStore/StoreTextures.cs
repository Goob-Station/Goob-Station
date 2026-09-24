using Content.Client.Resources;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Slasher.UI.VideoStore;

public static class StoreTextures
{
    public const string Dir = "/Textures/_Goobstation/Slasher/Interface/VideoStore/";

    public static Texture Get(IResourceCache cache, string name)
    {
        return cache.GetTexture(new ResPath(Dir + name + ".png"));
    }

    public static StyleBoxTexture NinePatch(IResourceCache cache, string name, float margin = 4f)
    {
        var box = new StyleBoxTexture { Texture = Get(cache, name) };
        box.SetPatchMargin(StyleBox.Margin.All, margin);
        box.SetContentMarginOverride(StyleBox.Margin.Horizontal, 8f);
        box.SetContentMarginOverride(StyleBox.Margin.Vertical, 4f);
        return box;
    }
}

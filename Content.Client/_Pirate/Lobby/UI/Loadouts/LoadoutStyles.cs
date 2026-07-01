using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.IoC;
using Robust.Shared.Maths;

namespace Content.Client._Pirate.Lobby.UI.Loadouts;

/// <summary>Rounded style boxes for the loadout UI.</summary>
public static class LoadoutStyles
{
    private static Texture? _bordered;
    private static Texture? _filled;

    private static Texture Bordered =>
        _bordered ??= IoCManager.Resolve<IResourceCache>().GetResource<TextureResource>("/Textures/Interface/Nano/rounded_button_bordered.svg.96dpi.png").Texture;

    private static Texture Filled =>
        _filled ??= IoCManager.Resolve<IResourceCache>().GetResource<TextureResource>("/Textures/Interface/Nano/rounded_button.svg.96dpi.png").Texture;

    // patchMargin also controls the default content inset.
    public static StyleBoxTexture RoundedBordered(Color modulate, float padding = 2f, float patchMargin = 5f)
    {
        return Build(Bordered, modulate, padding, patchMargin);
    }

    public static StyleBoxTexture RoundedFilled(Color modulate, float padding = 2f, float patchMargin = 5f)
    {
        return Build(Filled, modulate, padding, patchMargin);
    }

    private static StyleBoxTexture Build(Texture texture, Color modulate, float padding, float patchMargin)
    {
        var box = new StyleBoxTexture { Texture = texture, Modulate = modulate };
        box.SetPatchMargin(StyleBox.Margin.All, patchMargin);
        box.SetPadding(StyleBox.Margin.All, padding);
        return box;
    }
}

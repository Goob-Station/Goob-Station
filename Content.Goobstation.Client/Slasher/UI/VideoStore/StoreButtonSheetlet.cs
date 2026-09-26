using Content.Client.Resources;
using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Goobstation.Client.Slasher.UI.VideoStore;

[CommonSheetlet]
public sealed class StoreButtonSheetlet<T> : Sheetlet<T> where T : PalettedStylesheet
{
    public override StyleRule[] GetRules(T sheet, object config)
    {
        var button = StoreTextures.NinePatch(ResCache, "layer", 3f);
        var buttonCurrent = StoreTextures.NinePatch(ResCache, "layer_on", 3f);
        var buttonFont = ResCache.GetFont(StoreFonts.StencilPaths, 11);
        var arrowFont = ResCache.GetFont(StoreFonts.TermPaths, 16);
        var arrowLeft = StoreTextures.Get(ResCache, "arrow_l");
        var arrowRight = StoreTextures.Get(ResCache, "arrow_r");
        var lit = new Color(1.3f, 1.3f, 1.3f);

        return
        [
            LayerButton(StoreButtonStyles.Layer).Box(button),
            LayerButton(StoreButtonStyles.LayerCurrent).Box(buttonCurrent),
            LayerButton(StoreButtonStyles.Layer).ParentOf(E<Label>()).Font(buttonFont).FontColor(StorePalette.Muted),
            LayerButton(StoreButtonStyles.LayerCurrent).ParentOf(E<Label>()).Font(buttonFont).FontColor(StorePalette.Yellow),
            E<Label>().Class(StoreButtonStyles.LayerArrow).Font(arrowFont).FontColor(StorePalette.Steel),

            Arrow(StoreButtonStyles.ArrowLeft).Prop(TextureButton.StylePropertyTexture, arrowLeft),
            Arrow(StoreButtonStyles.ArrowLeft).Pseudo(TextureButton.StylePseudoClassHover).Modulate(lit),
            Arrow(StoreButtonStyles.ArrowRight).Prop(TextureButton.StylePropertyTexture, arrowRight),
            Arrow(StoreButtonStyles.ArrowRight).Pseudo(TextureButton.StylePseudoClassHover).Modulate(lit),
        ];
    }

    private static MutableSelectorElement LayerButton(string styleClass)
    {
        return E<Button>().Class(Button.StyleClassButton).Class(styleClass);
    }

    private static MutableSelectorElement Arrow(string styleClass)
    {
        return E<TextureButton>().Class(styleClass);
    }
}

public static class StoreButtonStyles
{
    public const string Layer = "StoreLayer";
    public const string LayerCurrent = "StoreLayerCurrent";
    public const string LayerArrow = "StoreLayerArrow";
    public const string ArrowLeft = "StoreArrowLeft";
    public const string ArrowRight = "StoreArrowRight";
}

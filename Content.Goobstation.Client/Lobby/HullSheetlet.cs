using Content.Client.Chat.UI;
using Content.Client.Resources;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Stylesheets;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Goobstation.Client.Lobby;

[CommonSheetlet]
public sealed class HullSheetlet : Sheetlet<NanotrasenStylesheet>
{
    private static readonly Color TextColor = Color.FromHex("#e9ecef");
    private static readonly Color MutedColor = Color.FromHex("#9aa1a8");
    private static readonly Color AmberColor = Color.FromHex("#e0a722");
    private static readonly Color LcdCaptionColor = Color.FromHex("#3fae3f");

    private const string ArtPath = "/Textures/_Goobstation/Interface/Lobby";
    private const float ArtPatch = 9;
    private const string FontPath = "/Fonts/_Goobstation/Bekoha";

    public override StyleRule[] GetRules(NanotrasenStylesheet sheet, object config)
    {
        var rules = new List<StyleRule>
        {
            E<PanelContainer>().Class(HullStyleClasses.Card).Panel(Flat("#262a2e", "#191b1e")),
            E<PanelContainer>().Class(HullStyleClasses.Inset).Panel(Flat("#111315", "#0a0b0c")),
            E<PanelContainer>().Class(HullStyleClasses.EdgePlate).Panel(Flat("#2c3035e8", "#15171a", padH: 7, padV: 3)),
            E<PanelContainer>().Class(HullStyleClasses.MenuPopup).Panel(Flat("#2c3035", "#15171a", padH: 2, padV: 2)),
            E<PanelContainer>().Class(HullStyleClasses.ChatInput).Panel(Flat("#1b1e21", "#2c3035", padH: 2, padV: 2)),
            E<PanelContainer>().Class(HullStyleClasses.Lcd).Panel(Flat("#0a120a", "#1a331a", thickness: 2, padH: 8, padV: 3)),
            E<PanelContainer>().Class(HullStyleClasses.Pin).Panel(Flat("#1a1810", "#e0a722")),

            E<Label>().Class(HullStyleClasses.Heading).Font(PixelFont(true)).FontColor(TextColor),
            E<Label>().Class(HullStyleClasses.Muted).Font(PixelFont(false, true)).FontColor(MutedColor),
            E<Label>().Class(HullStyleClasses.FactKey).Font(PixelFont(false, true)).FontColor(MutedColor),
            E<Label>().Class(HullStyleClasses.FactValue).Font(PixelFont(false)).FontColor(TextColor),
            E<Label>().Class(HullStyleClasses.LcdCaption).Font(RunechatStyle.GetFont(RunechatStyle.FontPath, 12)).FontColor(LcdCaptionColor),
            E<Label>().Class(HullStyleClasses.PinTitle).Font(PixelFont(true)).FontColor(AmberColor),
            E<Label>().Class(HullStyleClasses.PostDate).Font(PixelFont(false, true)).FontColor(MutedColor),
            E<Label>().Class(HullStyleClasses.PostTitle).Font(PixelFont(true)).FontColor(TextColor),
            E<Label>().Class(HullStyleClasses.PostNew).Font(PixelFont(true, true)).FontColor(AmberColor),
        };

        AddArtButton(rules, HullStyleClasses.Plate, "button", TextColor, PixelFont(false));
        AddButton(rules, HullStyleClasses.MenuItem, "#00000000", "#30353a", "#3a3f45", "#00000000", TextColor, PixelFont(false));
        AddArtButton(rules, HullStyleClasses.Tab, "button", MutedColor, PixelFont(true));
        AddArtButton(rules, HullStyleClasses.Danger, "danger", Color.White, PixelFont(false));

        AddEditorRules(rules, sheet);
        AddReadyArt(rules);

        rules.Add(E<ContainerButton>().Class(HullStyleClasses.Tab).PseudoPressed().ParentOf(E<Label>()).FontColor(TextColor));
        rules.Add(E<ContainerButton>().Class(HullStyleClasses.Tab).PseudoPressed().ParentOf(E()).ParentOf(E<Label>()).FontColor(TextColor));

        return rules.ToArray();
    }

    private static void AddEditorRules(List<StyleRule> rules, NanotrasenStylesheet sheet)
    {
        var cache = IoCManager.Resolve<IResourceCache>();
        var activeTab = ArtPatchBox(cache.GetTexture($"{ArtPath}/button_pressed.png"), 5, 4);
        var inactiveTab = ArtPatchBox(cache.GetTexture($"{ArtPath}/button.png"), 3, 6);

        rules.Add(E<TabContainer>().Class(HullStyleClasses.Tabs)
            .Prop(TabContainer.StylePropertyPanelStyleBox, Flat("#111315", "#0a0b0c", padH: 4, padV: 4))
            .Prop(TabContainer.StylePropertyTabStyleBox, activeTab)
            .Prop(TabContainer.StylePropertyTabStyleBoxInactive, inactiveTab)
            .Prop(TabContainer.stylePropertyTabFontColor, TextColor)
            .Prop(TabContainer.StylePropertyTabFontColorInactive, MutedColor)
            .Font(PixelFont(true)));

        rules.Add(E<LineEdit>().Class(HullStyleClasses.Field)
            .Prop(LineEdit.StylePropertyStyleBox, FieldBox(cache))
            .Font(PixelFont(false)));

        var grabber = ArtPatchBox(cache.GetTexture($"{ArtPath}/button.png"), 13, 13, 9);
        rules.Add(E<Slider>().Class(HullStyleClasses.Slider)
            .Prop(Slider.StylePropertyBackground, SliderFrame(cache))
            .Prop(Slider.StylePropertyForeground, new StyleBoxEmpty())
            .Prop(Slider.StylePropertyFill, SliderTrack("#8a6a1c"))
            .Prop(Slider.StylePropertyGrabber, grabber));

        var frame = new StyleBoxTexture { Texture = cache.GetTexture($"{ArtPath}/fact_panel_frame.png") };
        frame.SetPatchMargin(StyleBox.Margin.All, ArtPatch);

        rules.Add(E<Slider>().Class(HullStyleClasses.ColorTrack)
            .Prop(Slider.StylePropertyBackground, new StyleBoxEmpty())
            .Prop(Slider.StylePropertyForeground, frame)
            .Prop(Slider.StylePropertyFill, new StyleBoxEmpty())
            .Prop(Slider.StylePropertyGrabber, grabber));

        rules.Add(E<ColorableSlider>().Class(HullStyleClasses.ColorSlider)
            .Prop(Slider.StylePropertyBackground, new StyleBoxFlat(Color.White))
            .Prop(ColorableSlider.StylePropertyBackgroundWhite, new StyleBoxFlat(Color.White))
            .Prop(Slider.StylePropertyForeground, frame)
            .Prop(Slider.StylePropertyFill, new StyleBoxEmpty())
            .Prop(ColorableSlider.StylePropertyFillWhite, new StyleBoxFlat(Color.White))
            .Prop(Slider.StylePropertyGrabber, grabber));

        rules.Add(E<Label>().Class(HullStyleClasses.Section).Font(PixelFont(true)).FontColor(AmberColor));
        rules.Add(E<Label>().Class(HullStyleClasses.FieldLabel).Font(PixelFont(false)).FontColor(MutedColor));
        rules.Add(E<RichTextLabel>().Class(HullStyleClasses.FieldLabel).Font(PixelFont(false)));
        rules.Add(E<RichTextLabel>().Class(HullStyleClasses.TileName).Font(PixelFont(false, true)));
        rules.Add(E<Label>().Class(HullStyleClasses.Text).Font(PixelFont(false)).FontColor(TextColor));

        rules.Add(E<ContainerButton>().Class(HullStyleClasses.Slot).PseudoNormal().Box(ArtPatchBox(cache.GetTexture($"{ArtPath}/button.png"), 4, 7, 5)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(HullStyleClasses.Slot).PseudoHovered().Box(ArtPatchBox(cache.GetTexture($"{ArtPath}/button_hover.png"), 4, 7, 5)).Modulate(Color.White));
        var selectedSlot = ArtPatchBox(cache.GetTexture($"{ArtPath}/button_tint_pressed.png"), 6, 5, 5);
        selectedSlot.Modulate = HullPalette.Green;
        rules.Add(E<ContainerButton>().Class(HullStyleClasses.Slot).PseudoPressed().Box(selectedSlot).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(HullStyleClasses.Slot).PseudoDisabled().Box(ArtPatchBox(cache.GetTexture($"{ArtPath}/button_disabled.png"), 4, 7, 5)).Modulate(Color.White));
    }

    private static void AddReadyArt(List<StyleRule> rules)
    {
        var cache = IoCManager.Resolve<IResourceCache>();

        AddImageButton(rules, cache, HullStyleClasses.ReadyArt, "ready");
        AddImageButton(rules, cache, HullStyleClasses.JoinArt, "join");
        AddImageButton(rules, cache, HullStyleClasses.SaveArt, "save");

        var roundPanel = new StyleBoxTexture
        {
            Texture = cache.GetTexture("/Textures/_Goobstation/Interface/Lobby/round_panel.png"),
        };
        roundPanel.SetPatchMargin(StyleBox.Margin.All, 8);
        roundPanel.SetContentMarginOverride(StyleBox.Margin.All, 8);
        rules.Add(E<PanelContainer>().Class(HullStyleClasses.RoundPanel).Panel(roundPanel));

        var factPanel = new StyleBoxTexture { Texture = cache.GetTexture($"{ArtPath}/fact_panel.png") };
        factPanel.SetPatchMargin(StyleBox.Margin.All, ArtPatch);
        factPanel.SetContentMarginOverride(StyleBox.Margin.Horizontal, 7);
        factPanel.SetContentMarginOverride(StyleBox.Margin.Vertical, 4);
        rules.Add(E<PanelContainer>().Class(HullStyleClasses.Fact).Panel(factPanel));

        static void AddImageButton(List<StyleRule> rules, IResourceCache cache, string styleClass, string prefix)
        {
            var normal = cache.GetTexture($"{ArtPath}/{prefix}.png");
            var pressed = cache.GetTexture($"{ArtPath}/{prefix}_pressed.png");

            rules.Add(E<ContainerButton>().Class(styleClass).PseudoNormal().Box(Art(normal, Color.White)).Modulate(Color.White));
            rules.Add(E<ContainerButton>().Class(styleClass).PseudoHovered().Box(Art(normal, Color.FromHex("#e2e2e2"))).Modulate(Color.White));
            rules.Add(E<ContainerButton>().Class(styleClass).PseudoPressed().Box(Art(pressed, Color.White)).Modulate(Color.White));
            rules.Add(E<ContainerButton>().Class(styleClass).PseudoDisabled().Box(Art(normal, Color.FromHex("#777777"))).Modulate(Color.White));
            rules.Add(E<ContainerButton>().Class(styleClass).ParentOf(E<Label>()).FontColor(Color.Transparent));
            rules.Add(E<ContainerButton>().Class(styleClass).PseudoNormal().ParentOf(E<Label>()).FontColor(Color.Transparent));
            rules.Add(E<ContainerButton>().Class(styleClass).PseudoHovered().ParentOf(E<Label>()).FontColor(Color.Transparent));
            rules.Add(E<ContainerButton>().Class(styleClass).PseudoPressed().ParentOf(E<Label>()).FontColor(Color.Transparent));
            rules.Add(E<ContainerButton>().Class(styleClass).PseudoDisabled().ParentOf(E<Label>()).FontColor(Color.Transparent));
        }

        static StyleBoxTexture Art(Texture texture, Color modulate)
        {
            var box = new StyleBoxTexture { Texture = texture, Modulate = modulate };
            box.SetContentMarginOverride(StyleBox.Margin.All, 0);
            return box;
        }
    }

    private static void AddArtButton(List<StyleRule> rules, string styleClass, string prefix, Color text, Font font)
    {
        var cache = IoCManager.Resolve<IResourceCache>();

        rules.Add(E<ContainerButton>().Class(styleClass).PseudoNormal().Box(ArtPatchBox(cache.GetTexture($"{ArtPath}/{prefix}.png"), 3, 6)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(styleClass).PseudoHovered().Box(ArtPatchBox(cache.GetTexture($"{ArtPath}/{prefix}_hover.png"), 3, 6)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(styleClass).PseudoPressed().Box(ArtPatchBox(cache.GetTexture($"{ArtPath}/{prefix}_pressed.png"), 5, 4)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(styleClass).PseudoDisabled().Box(ArtPatchBox(cache.GetTexture($"{ArtPath}/{prefix}_disabled.png"), 3, 6)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(styleClass).ParentOf(E<Label>()).FontColor(text).Font(font));
        rules.Add(E<ContainerButton>().Class(styleClass).ParentOf(E()).ParentOf(E<Label>()).FontColor(text).Font(font));
    }

    private static StyleBoxTexture ArtPatchBox(Texture texture, float top, float bottom, float horizontal = 8)
    {
        var box = new StyleBoxTexture { Texture = texture };
        box.SetPatchMargin(StyleBox.Margin.All, ArtPatch);
        box.SetContentMarginOverride(StyleBox.Margin.Horizontal, horizontal);
        box.SetContentMarginOverride(StyleBox.Margin.Top, top);
        box.SetContentMarginOverride(StyleBox.Margin.Bottom, bottom);
        return box;
    }

    private static void AddButton(List<StyleRule> rules, string styleClass, string normal, string hovered, string pressed, string disabled, Color text, Font font)
    {
        rules.Add(E<ContainerButton>().Class(styleClass).PseudoNormal().Box(Flat(normal, "#15171a", padH: 8, padV: 3)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(styleClass).PseudoHovered().Box(Flat(hovered, "#15171a", padH: 8, padV: 3)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(styleClass).PseudoPressed().Box(Flat(pressed, "#15171a", padH: 8, padV: 3)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(styleClass).PseudoDisabled().Box(Flat(disabled, "#15171a", padH: 8, padV: 3)).Modulate(Color.White));
        rules.Add(E<ContainerButton>().Class(styleClass).ParentOf(E<Label>()).FontColor(text).Font(font));
        rules.Add(E<ContainerButton>().Class(styleClass).ParentOf(E()).ParentOf(E<Label>()).FontColor(text).Font(font));
    }

    private static StyleBoxTexture FieldBox(IResourceCache cache)
    {
        var box = new StyleBoxTexture { Texture = cache.GetTexture($"{ArtPath}/fact_panel.png") };
        box.SetPatchMargin(StyleBox.Margin.All, ArtPatch);
        box.SetContentMarginOverride(StyleBox.Margin.Horizontal, 7);
        box.SetContentMarginOverride(StyleBox.Margin.Vertical, 4);
        return box;
    }

    private static StyleBoxTexture SliderFrame(IResourceCache cache)
    {
        var box = new StyleBoxTexture { Texture = cache.GetTexture($"{ArtPath}/fact_panel.png") };
        box.SetPatchMargin(StyleBox.Margin.All, ArtPatch);
        return box;
    }

    private static StyleBoxTexture SliderTrack(string color)
    {
        var box = new StyleBoxTexture
        {
            Texture = Texture.White,
            Modulate = Color.FromHex(color),
        };
        box.SetExpandMargin(StyleBox.Margin.Vertical, -9);
        box.SetExpandMargin(StyleBox.Margin.Left, -5);
        return box;
    }

    private static Font PixelFont(bool bold, bool small = false, bool italic = false)
    {
        var style = (bold, italic) switch
        {
            (true, true) => "-BoldItalic",
            (true, false) => "-Bold",
            (false, true) => "-Italic",
            _ => string.Empty,
        };

        var cache = IoCManager.Resolve<IResourceCache>();
        var pixel = cache.GetFont($"{FontPath}/Freezeframe-Cond{style}.ttf", small ? 6 : 12);
        var fallback = cache.NotoStack(bold ? "Bold" : "Regular", small ? 7 : 11);
        return new StackedFont(pixel, fallback);
    }

    private static StyleBoxFlat Flat(string background, string border, float thickness = 1, float padH = 0, float padV = 0)
    {
        var box = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex(background),
            BorderColor = Color.FromHex(border),
            BorderThickness = new Thickness(thickness),
        };
        box.SetContentMarginOverride(StyleBox.Margin.Horizontal, padH + thickness);
        box.SetContentMarginOverride(StyleBox.Margin.Vertical, padV + thickness);
        return box;
    }
}

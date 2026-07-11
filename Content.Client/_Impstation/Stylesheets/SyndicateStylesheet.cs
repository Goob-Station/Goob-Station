using System.Linq;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Fonts;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.StylesheetHelpers;

namespace Content.Client._Impstation.Stylesheets;

[Virtual]
public partial class SyndicateStylesheet : CommonStylesheet
{
    public override string StylesheetName => "Syndicate";

    public override NotoFontFamilyStack BaseFont { get; }

    public override Dictionary<Type, ResPath[]> Roots => new()
    {
        { typeof(TextureResource), [] },
    };

    private const int PrimaryFontSize = 12;
    private const int FontSizeStep = 2;

    private readonly List<(string?, int)> _commonFontSizes = new()
    {
        (null, PrimaryFontSize),
        (StyleClass.FontSmall, PrimaryFontSize - FontSizeStep),
        (StyleClass.FontLarge, PrimaryFontSize + FontSizeStep),
    };

    public SyndicateStylesheet(object config, StylesheetManager man) : base(config)
    {
        BaseFont = new NotoFontFamilyStack(ResCache);
        var rules = new[]
        {
            GetRulesForFont(null, BaseFont, _commonFontSizes),
            [
                Element().Prop(Label.StylePropertyFont, BaseFont.GetFont(PrimaryFontSize)),
            ],
            GetAllSheetletRules<PalettedStylesheet, CommonSheetletAttribute>(man),
            GetAllSheetletRules<SyndicateStylesheet, CommonSheetletAttribute>(man),
        };

        Stylesheet = new Stylesheet(rules.SelectMany(x => x).ToArray());
    }
}

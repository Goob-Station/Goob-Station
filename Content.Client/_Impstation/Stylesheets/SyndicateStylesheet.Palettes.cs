using Content.Client.Stylesheets.Palette;

namespace Content.Client._Impstation.Stylesheets;

public sealed partial class SyndicateStylesheet
{
    public override ColorPalette PrimaryPalette => Palettes.Maroon;
    public override ColorPalette SecondaryPalette => ColorPalette.FromHexBase("#303030");
    public override ColorPalette PositivePalette => Palettes.Green;
    public override ColorPalette NegativePalette => Palettes.Red;
    public override ColorPalette HighlightPalette => Palettes.Maroon;
}

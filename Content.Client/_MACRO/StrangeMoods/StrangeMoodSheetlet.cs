using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._MACRO.StrangeMoods;

[CommonSheetlet]
public sealed class StrangeMoodSheetlet : Sheetlet<NanotrasenStylesheet>
{
    public override StyleRule[] GetRules(NanotrasenStylesheet sheet, object config)
    {
        return
        [
            E<LineEdit>()
                .Identifier("StrangeMoodShared")
                .FontColor(sheet.SecondaryPalette.Text),
            E<TextEdit>()
                .Identifier("StrangeMoodShared")
                .FontColor(sheet.SecondaryPalette.Text),
        ];
    }
}
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Fonts;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Content.Client.Stylesheets.StylesheetHelpers;

namespace Content.Client._Pirate.Traits.UI;

/// <summary>
/// Active sheetlet replacement for the Pirate trait rules that previously lived in legacy StyleNano.
/// </summary>
[CommonSheetlet]
public sealed class TraitsSheetlet : Sheetlet<PalettedStylesheet>
{
    public override StyleRule[] GetRules(PalettedStylesheet sheet, object config)
    {
        var font10 = sheet.BaseFont.GetFont(10);
        var font11 = sheet.BaseFont.GetFont(11);
        var font12 = sheet.BaseFont.GetFont(12);
        var displayBold14 = sheet.BaseFont.GetFont(14, FontKind.Bold);

        var progressBarBackground = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#1a1a22"),
            BorderColor = Color.FromHex("#32323e"),
            BorderThickness = new Thickness(1),
        };
        var categoryHeader = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#2a2a35"),
            BorderColor = Color.FromHex("#32323e"),
            BorderThickness = new Thickness(0, 0, 0, 1),
        };
        var categoryContent = new StyleBoxFlat { BackgroundColor = Color.FromHex("#22222a") };
        var entryDisabled = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#1a1a22"),
            BorderColor = Color.FromHex("#2a2a2a"),
            BorderThickness = new Thickness(1),
        };
        var entryPanel = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#2a2a35"),
            BorderColor = Color.FromHex("#32323e"),
            BorderThickness = new Thickness(1),
        };
        var entrySelected = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#2a3a4a"),
            BorderColor = Color.FromHex("#60a5fa"),
            BorderThickness = new Thickness(1),
        };
        var entryUnavailable = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#2b1a1a"),
            BorderColor = Color.FromHex("#f87171"),
            BorderThickness = new Thickness(1),
        };
        var entryUnaffordable = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#2b2a1a"),
            BorderColor = Color.FromHex("#fbbf24"),
            BorderThickness = new Thickness(1),
        };

        return
        [
            E<PanelContainer>().Class("TraitsHeaderPanel").Panel(new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#2a2a35"),
                BorderColor = Color.FromHex("#32323e"),
                BorderThickness = new Thickness(0, 0, 0, 1),
            }),
            E<Label>().Class("TraitsTitleLabel").Font(displayBold14).FontColor(Color.FromHex("#e0e0e0")),
            E<Label>().Class("TraitsSubtitleLabel").Font(font11).FontColor(Color.FromHex("#a0a0a0")),
            E<Label>().Class("TraitsStatLabel").Font(font12).FontColor(Color.FromHex("#60a5fa")),

            E<PanelContainer>().Class("TraitsProgressBarBg").Panel(progressBarBackground),
            E<PanelContainer>().Class("TraitsProgressBarFill").Panel(new StyleBoxFlat(Color.FromHex("#4ade80"))),
            E<PanelContainer>().Class("TraitsProgressBarFull").Panel(new StyleBoxFlat(Color.FromHex("#4ade80"))),
            E<PanelContainer>().Class("TraitsProgressBarPartial").Panel(new StyleBoxFlat(Color.FromHex("#fbbf24"))),
            E<PanelContainer>().Class("TraitsProgressBarLow").Panel(new StyleBoxFlat(Color.FromHex("#f87171"))),
            E<PanelContainer>().Class("TraitsProgressBarEmpty").Panel(new StyleBoxFlat(Color.FromHex("#1a1a22"))),

            E<PanelContainer>().Class("TraitsSearchBar").Panel(new StyleBoxFlat(Color.FromHex("#22222a"))),
            E<LineEdit>().Class("TraitsSearchInput").Box(new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#1a1a22"),
                ContentMarginLeftOverride = 8,
                ContentMarginRightOverride = 8,
            }),

            E<PanelContainer>().Class("TraitsFooterPanel").Panel(new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#22222a"),
                BorderColor = Color.FromHex("#32323e"),
                BorderThickness = new Thickness(0, 1, 0, 0),
            }),
            E<Label>().Class("TraitsFooterText").Font(font10).FontColor(Color.FromHex("#707070")),

            E<PanelContainer>().Class("TraitsCategoryHeader").Panel(categoryHeader),
            E<Button>().Class("TraitsCategoryHeaderButton").Box(new StyleBoxFlat { BackgroundColor = Color.Transparent }),
            E<Label>().Class("TraitsCategoryExpandIcon").Font(font10).FontColor(Color.FromHex("#a0a0a0")),
            E<Label>().Class("TraitsCategoryNameLabel").Font(font12).FontColor(Color.FromHex("#e0e0e0")),
            E<Label>().Class("TraitsCategoryStatsLabel").Font(font10).FontColor(Color.FromHex("#a0a0a0")),
            E<Label>().Class("TraitsCategoryPointsLabel").Font(font10).FontColor(Color.FromHex("#707070")),
            E<PanelContainer>().Class("TraitsCategoryAccent").Panel(new StyleBoxFlat(Color.FromHex("#60a5fa"))),
            E<PanelContainer>().Class("TraitsCategoryContent").Panel(categoryContent),

            E<PanelContainer>().Class("TraitsEntryPanel").Panel(entryPanel),
            E<PanelContainer>().Class("TraitsEntryPanel", "TraitsEntrySelected").Panel(entrySelected),
            E<PanelContainer>()
                .Class("TraitsEntryPanel", "TraitsEntryDisabled")
                .Panel(entryDisabled)
                .Modulate(new Color(1f, 1f, 1f, 0.5f)),
            E<PanelContainer>().Class("TraitsEntryPanel", "TraitsEntryUnavailable").Panel(entryUnavailable),
            E<PanelContainer>().Class("TraitsEntryPanel", "TraitsEntryUnaffordable").Panel(entryUnaffordable),
            E<Label>().Class("TraitsEntryNameLabel").Font(font11).FontColor(Color.FromHex("#e0e0e0")),

            E().Class("TraitsEntryUnavailable")
                .ParentOf(E<Label>().Class("TraitsEntryNameLabel"))
                .FontColor(Color.FromHex("#f87171")),
            E().Class("TraitsEntryUnavailable")
                .ParentOf(E<RichTextLabel>().Class("TraitsEntryDescriptionLabel"))
                .Modulate(Color.FromHex("#f87171").WithAlpha(0.7f)),
            E().Class("TraitsEntryUnaffordable")
                .ParentOf(E<Label>().Class("TraitsEntryNameLabel"))
                .FontColor(Color.FromHex("#fbbf24")),
            E().Class("TraitsEntryUnaffordable")
                .ParentOf(E<RichTextLabel>().Class("TraitsEntryDescriptionLabel"))
                .Modulate(Color.FromHex("#fbbf24").WithAlpha(0.7f)),

            E<Label>().Class("TraitsEntryCostLabel").Font(font11),
            E<RichTextLabel>()
                .Class("TraitsEntryDescriptionLabel")
                .Font(font10)
                .FontColor(Color.FromHex("#a0a0a0")),
        ];
    }
}

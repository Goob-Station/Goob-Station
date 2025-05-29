using Robust.Shared.Configuration;

namespace Content.Casino.Shared.Cvars;

[CVarDefs]
public sealed partial class CasinoCVars
{
    #region Blackjack

    /// <summary>
    /// How many decks are used for a blackjack game
    /// </summary>
    public static readonly CVarDef<int> BlackjackDeckCount =
        CVarDef.Create("blackjack.deckcount", 6, CVar.SERVERONLY);

    /// <summary>
    /// Player wins if hand has five cards without going bust
    /// This provides a significant player edge
    /// </summary>
    public static readonly CVarDef<bool> BlackjackFiveCardWin =
        CVarDef.Create("blackjack.FiveCardWin", false, CVar.SERVERONLY);

    /// <summary>
    /// Allow splitting your hand
    /// </summary>
    public static readonly CVarDef<bool> BlackjackSplitAllowed =
        CVarDef.Create("blackjack.SplitAllowed", true, CVar.SERVERONLY);

    /// <summary>
    /// Dealer hits on Soft (Ace counts as 11 and 1) 17
    /// In general this provides a very small house edge.
    /// </summary>
    public static readonly CVarDef<bool> BlackjackDealerHitsSoft17 =
        CVarDef.Create("blackjack.DealerHitsSoft17", true, CVar.SERVERONLY);

    #endregion
}

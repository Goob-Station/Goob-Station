using Robust.Shared.Configuration;

namespace Content.Casino.Shared.Cvars;

[CVarDefs]
public sealed partial class CasinoCVars
{
    /// <summary>
    /// Born too late to explore the earth, born too early to explore the cosmos.
    /// All that you have left is gambling your daughter's college fund away.
    /// </summary>
    public static readonly CVarDef<bool> CasinoEnabled =
        CVarDef.Create("casino.enabled", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Announce to everyone that someone won big
    /// </summary>
    public static readonly CVarDef<bool> AnnounceBigWins =
        CVarDef.Create("casino.BigWinAnnounce", true, CVar.SERVER);

    /// <summary>
    /// Consider a win that gives this amount or more a "big win"
    /// </summary>
    public static readonly CVarDef<int> BigWinThreshold =
        CVarDef.Create("casino.BigWinThreshold", 5000, CVar.SERVER);

    #region Blackjack

    /// <summary>
    /// How many decks are used for a blackjack game
    /// </summary>
    public static readonly CVarDef<int> BlackjackDeckCount =
        CVarDef.Create("blackjack.DeckCount", 6, CVar.SERVERONLY);

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

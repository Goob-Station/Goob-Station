using System.Collections.Generic;

namespace Content.Casino.Shared.Games.Blackjack;

public enum BlackjackPhase
{
    Dealing,
    PlayerTurn,
    DealerTurn,
    GameOver
}

public enum HandStatus
{
    Active,
    Stand,
    Bust,
    Blackjack,
    Doubled
}

public sealed record BlackjackHand(
    List<Card> Cards,
    int Bet,
    HandStatus Status = HandStatus.Active,
    bool HasInsurance = false
)
{
    public int Value => Cards.GetBestValue();
    public bool IsBlackjack => Cards.IsBlackjack();
    public bool IsBust => Cards.IsBust();
    public bool IsSoft => Cards.IsSoft();
    public bool CanSplit => Cards.Count == 2 && Cards[0].Rank == Cards[1].Rank;
    public bool CanDoubleDown => Cards.Count == 2 && Status == HandStatus.Active;

    public string Display => Cards.GetHandDisplay();
}

public sealed record BlackjackGameState(
    List<Card> DealerHand,
    List<BlackjackHand> PlayerHands,
    int CurrentHandIndex,
    BlackjackPhase Phase,
    Deck GameDeck,
    int BaseBet,
    bool DealerHasBlackjack = false,
    bool InsuranceOffered = false,
    bool GameComplete = false
)
{
    public BlackjackHand CurrentHand => CurrentHandIndex < PlayerHands.Count ? PlayerHands[CurrentHandIndex] : null!;
    public Card DealerUpCard => DealerHand.Count > 0 ? DealerHand[0] : default;
    public bool DealerHasAce => DealerUpCard.IsAce;
    public int DealerValue => DealerHand.GetBestValue();
    public string DealerDisplay => DealerHand.GetHandDisplay(Phase != BlackjackPhase.DealerTurn && Phase != BlackjackPhase.GameOver);

    public bool CanTakeInsurance => InsuranceOffered && !CurrentHand.HasInsurance && DealerHasAce;

    public bool HasActiveHands => PlayerHands.Exists(h => h.Status == HandStatus.Active);

    public bool AllHandsComplete => PlayerHands.TrueForAll(h =>
        h.Status == HandStatus.Stand ||
        h.Status == HandStatus.Bust ||
        h.Status == HandStatus.Blackjack ||
        h.Status == HandStatus.Doubled);
}

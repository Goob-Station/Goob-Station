using System.Linq;

namespace Content.Casino.Shared.Games.Blackjack;

public enum Suit : byte
{
    Hearts = 0,
    Diamonds = 1,
    Clubs = 2,
    Spades = 3
}

public enum Rank : byte
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14
}

public readonly record struct Card(Suit Suit, Rank Rank)
{
    public int GetBlackjackValue(bool aceAsEleven = true)
    {
        return Rank switch
        {
            Rank.Ace => aceAsEleven ? 11 : 1,
            Rank.Jack or Rank.Queen or Rank.King => 10,
            _ => (int)Rank
        };
    }

    public bool IsAce => Rank == Rank.Ace;
    public bool IsTen => GetBlackjackValue() == 10;

    public override string ToString()
    {
        var rankStr = Rank switch
        {
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            _ => ((int)Rank).ToString()
        };

        var suitStr = Suit switch
        {
            Suit.Hearts => "H",
            Suit.Diamonds => "D",
            Suit.Clubs => "C",
            Suit.Spades => "S",
            _ => "?"
        };

        return $"{rankStr}{suitStr}";
    }
}

public static class HandExtensions
{
    public static int GetBestValue(this IReadOnlyList<Card> hand)
    {
        var aces = hand.Count(c => c.IsAce);
        var nonAceValue = hand.Where(c => !c.IsAce).Sum(c => c.GetBlackjackValue());

        // Start with all aces as 11
        var totalValue = nonAceValue + (aces * 11);

        // Convert aces from 11 to 1 until we're under 22 or run out of aces
        while (totalValue > 21 && aces > 0)
        {
            totalValue -= 10; // Convert one ace from 11 to 1
            aces--;
        }

        return totalValue;
    }

    public static bool IsBlackjack(this IReadOnlyList<Card> hand)
    {
        return hand.Count == 2 && hand.GetBestValue() == 21;
    }

    public static bool IsBust(this IReadOnlyList<Card> hand)
    {
        return hand.GetBestValue() > 21;
    }

    public static bool IsSoft(this IReadOnlyList<Card> hand)
    {
        var aces = hand.Count(c => c.IsAce);
        if (aces == 0) return false;

        var nonAceValue = hand.Where(c => !c.IsAce).Sum(c => c.GetBlackjackValue());
        return nonAceValue + 11 + (aces - 1) <= 21;
    }

    public static string GetHandDisplay(this IReadOnlyList<Card> hand, bool hideHoleCard = false)
    {
        if (hand.Count == 0) return "[]";

        var cards = new List<string>();
        for (int i = 0; i < hand.Count; i++)
        {
            if (hideHoleCard && i == 1)
                cards.Add("XX");
            else
                cards.Add(hand[i].ToString());
        }

        return $"[{string.Join(", ", cards)}]";
    }
}

public sealed class Deck
{
    private readonly List<Card> _cards = new();
    private readonly Random _random;

    public Deck(Random random, int numDecks = 6)
    {
        _random = random;
        InitializeDeck(numDecks);
        Shuffle();
    }

    private void InitializeDeck(int numDecks)
    {
        _cards.Clear();

        for (int deck = 0; deck < numDecks; deck++)
        {
            foreach (Suit suit in Enum.GetValues<Suit>())
            {
                foreach (Rank rank in Enum.GetValues<Rank>())
                {
                    _cards.Add(new Card(suit, rank));
                }
            }
        }
    }

    public void Shuffle()
    {
        for (int i = _cards.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
        }
    }

    public Card DrawCard()
    {
        if (_cards.Count == 0)
            throw new InvalidOperationException("Cannot draw from empty deck");

        var card = _cards[^1];
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }

    public bool NeedsReshuffle => _cards.Count < 50; // Reshuffle when less than 50 cards remain
}

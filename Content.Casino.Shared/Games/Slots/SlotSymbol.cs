using System;
using System.IO;
using System.Linq;
using Robust.Shared.IoC;
using Robust.Shared.Serialization;

namespace Content.Casino.Shared.Games.Slots;

public enum SlotSymbol : byte
{
    Cherry = 0,
    Lemon = 1,
    Orange = 2,
    Plum = 3,
    Bell = 4,
    Bar = 5,
    Seven = 6
}

public sealed record SlotGameState(
    SlotSymbol[] Symbols,
    int CurrentBet,
    bool IsSpinning = false
)
{
    public static SlotGameState FromJson(string json)
    {
        try
        {
            var serializer = IoCManager.Resolve<IRobustSerializer>();
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            return serializer.Deserialize<SlotGameState>(stream) ??
                   throw new InvalidOperationException("Failed to deserialize slot game state");
        }
        catch (Exception)
        {
            // Return default state if deserialization fails
            return new SlotGameState(Array.Empty<SlotSymbol>(), 0, false);
        }
    }

    public string ToJson()
    {
        try
        {
            var serializer = IoCManager.Resolve<IRobustSerializer>();
            using var stream = new MemoryStream();
            serializer.Serialize(stream, this);
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (Exception)
        {
            // Return empty JSON object if serialization fails
            return "{}";
        }
    }
}

public static class SlotSymbolExtensions
{
    private static readonly string[] SymbolDisplayNames =
    {
        "Cherry", // Cherry
        "Lemon", // Lemon
        "Orange", // Orange
        "Plum", // Plum
        "Bell", // Bell
        "Bar", // Bar
        "Seven",  // Seven
    };

    /// <summary>
    /// Get the display representation of a slot symbol.
    /// </summary>
    public static string GetDisplayName(this SlotSymbol symbol)
    {
        return SymbolDisplayNames[(int)symbol];
    }

    /// <summary>
    /// Calculate the payout for an array of symbols.
    /// </summary>
    public static int GetPayout(this SlotSymbol[] symbols, int bet)
    {
        if (symbols.Length != 3)
            throw new ArgumentException("Slot machine must have exactly 3 symbols", nameof(symbols));

        // Check for three of a kind (jackpot)
        if (symbols[0] == symbols[1] && symbols[1] == symbols[2])
        {
            return symbols[0] switch
            {
                SlotSymbol.Cherry => bet * 2,
                SlotSymbol.Lemon => bet * 3,
                SlotSymbol.Orange => bet * 4,
                SlotSymbol.Plum => bet * 5,
                SlotSymbol.Bell => bet * 8,
                SlotSymbol.Bar => bet * 10,
                SlotSymbol.Seven => bet * 20,
                _ => 0
            };
        }

        // Check for two of a kind (smaller payout)
        if (symbols[0] == symbols[1] || symbols[1] == symbols[2] || symbols[0] == symbols[2])
        {
            // Find the matching symbol
            var matchingSymbol = symbols[0] == symbols[1] ? symbols[0] :
                               symbols[1] == symbols[2] ? symbols[1] : symbols[0];

            return matchingSymbol switch
            {
                SlotSymbol.Seven => bet, // Two sevens still pays even money
                SlotSymbol.Bar => bet,
                SlotSymbol.Bell => (int)(bet * 0.5f),
                _ => 0 // Other symbols don't pay for two of a kind
            };
        }

        return 0; // No winning combination
    }

    /// <summary>
    /// Get a formatted display string for the symbols.
    /// </summary>
    public static string GetDisplayString(this SlotSymbol[] symbols)
    {
        if (symbols.Length != 3)
            return string.Join(" | ", symbols.Select(s => s.GetDisplayName()));

        return $"[ {symbols[0].GetDisplayName()} | {symbols[1].GetDisplayName()} | {symbols[2].GetDisplayName()} ]";
    }

    /// <summary>
    /// Check if the symbols represent a winning combination.
    /// </summary>
    public static bool IsWinningCombination(this SlotSymbol[] symbols)
    {
        return symbols.GetPayout(1) > 0; // Use bet of 1 to check if there's any payout
    }

    /// <summary>
    /// Get the type of win for display purposes.
    /// </summary>
    public static string GetWinType(this SlotSymbol[] symbols)
    {
        if (symbols.Length != 3)
            return "No Win";

        // Three of a kind
        if (symbols[0] == symbols[1] && symbols[1] == symbols[2])
        {
            return symbols[0] switch
            {
                SlotSymbol.Seven => "JACKPOT! Triple Sevens!",
                SlotSymbol.Bar => "Triple Bars!",
                SlotSymbol.Bell => "Triple Bells!",
                _ => $"Triple {symbols[0]}s!"
            };
        }

        // Two of a kind
        if (symbols[0] == symbols[1] || symbols[1] == symbols[2] || symbols[0] == symbols[2])
        {
            var matchingSymbol = symbols[0] == symbols[1] ? symbols[0] :
                               symbols[1] == symbols[2] ? symbols[1] : symbols[0];

            if (matchingSymbol == SlotSymbol.Seven || matchingSymbol == SlotSymbol.Bar || matchingSymbol == SlotSymbol.Bell)
            {
                return $"Pair of {matchingSymbol}s!";
            }
        }

        return "No Win";
    }
}

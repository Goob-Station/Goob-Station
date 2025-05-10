namespace Content.Casino.Shared.Games.Slots;

public enum SlotSymbol
{
    Cherry,
    Lemon,
    Orange,
    Plum,
    Bell,
    Bar,
    Seven
}

public static class SlotSymbolExtensions
{
    public static int GetPayout(this SlotSymbol[] symbols, int bet)
    {
        // Check for three of a kind
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

        // Check for two of a kind
        if (symbols[0] == symbols[1] || symbols[1] == symbols[2] || symbols[0] == symbols[2])
        {
            return bet;
        }

        return 0;
    }
}

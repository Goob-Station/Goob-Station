using System.Text;
using Robust.Shared.Random;

namespace Content.Shared.Humanoid;

public sealed partial class NamingSystem : EntitySystem
{
    private static readonly Dictionary<string, int> RomanMap = new()
    {
        { "M", 1000 },
        { "CM", 900 },
        { "D", 500 },
        { "CD", 400 },
        { "C", 100 },
        { "XC", 90 },
        { "L", 50 },
        { "XL", 40 },
        { "X", 10 },
        { "IX", 9 },
        { "V", 5 },
        { "IV", 4 },
        { "I", 1 }
    };

    // <summary>
    //   Generates a random Roman numeral with a length not exceeding 8 characters.
    //   All possible Roman numerals from 1 to 3,999 can be generated,
    //   but numbers from 1 to 100 have a higher chance of being rolled.
    // </summary>
    private string GenerateRomanNumeral()
    {
        (int, int) range;
        while (true)
        {
            if (_random.Prob(0.8f))       // 80% chance for 1-100
                range = (1, 101);
            else if (_random.Prob(0.6f))  // 12% chance for 101-500
                range = (101, 501);
            else if (_random.Prob(0.75f)) //  6% chance for 501-1,000
                range = (501, 1001);
            else                          //  2% chance for 1,001-3,999
                range = (1001, 4000);

            var numeral = IntToRomanNumeral(_random.Next(range.Item1, range.Item2));

            // Reroll when the numeral length is greater than 8 to prevent lengthy Roman numerals
            if (numeral.Length > 8)
                continue;

            return numeral;
        }
    }

    // <summary>
    //   Converts an integer to a Roman numeral.
    // </summary>
    private static string IntToRomanNumeral(int number)
    {
        var sb = new StringBuilder();
        foreach (var (letters, equivalentNumber) in RomanMap)
        {
            while (number >= equivalentNumber)
            {
                sb.Append(letters);
                number -= equivalentNumber;
            }
        }
        return sb.ToString();
    }
}

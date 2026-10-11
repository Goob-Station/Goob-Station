using Robust.Shared.Prototypes;

namespace Content.Server.Holiday.Christmas;

public sealed partial class RandomGiftSystem
{
    /// <summary>
    /// [Goob]
    /// public function
    /// </summary>
    /// <param name="itemOnly"></param>
    /// <returns></returns>
    public List<string> GetGifts(bool itemOnly = true)
    {
        return itemOnly ? _possibleGiftsSafe : _possibleGiftsUnsafe;
    }
}
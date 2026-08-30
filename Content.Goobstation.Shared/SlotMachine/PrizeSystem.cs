using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.SlotMachine;

/// <summary>
/// Used for getting a weighted random prize from a list of prizes
/// </summary>
public sealed partial class PrizeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    /// <summary>
    /// Ts is taken from SharedRandomExtensions so don't blame me if the math isn't right
    /// </summary>
    /// <param name="prizes">List of prize prototypes to pick from</param>
    /// <returns></returns>
    public PrizePrototype GetRandomPrize(List<ProtoId<PrizePrototype>> prizes)
    {
        Dictionary<PrizePrototype, float> picks = new();

        foreach (var prize in prizes)
        {
            var proto = _proto.Index(prize);

            picks[proto] = proto.Weight;
        }

        var sum = picks.Values.Sum();
        var accumulated = 0f;

        var rand = _random.NextFloat() * sum;

        foreach (var (prize, weight) in picks)
        {
            accumulated += weight;

            if (accumulated >= rand)
            {
                return prize;
            }
        }

        return _proto.Index(prizes[0]); // Shouldn't be possible but just incase
    }
}

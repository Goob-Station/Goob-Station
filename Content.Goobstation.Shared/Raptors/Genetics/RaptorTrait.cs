using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Raptors.Genetics
{
    /// <summary>
    /// Personality traits affecting behavior.
    /// </summary>
    [Serializable, NetSerializable]
    public enum RaptorTrait
    {
        Playful,
        Motherly,
        Depressed,
        Coward,
        TroubleMaker
    }
}

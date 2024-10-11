using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Changeling;

[Prototype]
public sealed partial class StingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public LocId Name { get; private set; }

    /// <summary>
    ///     Time after which effect will occur after sting use.
    /// </summary>
    [DataField]
    public float UseLatency { get; private set; }

    [DataField]
    public SpriteSpecifier Icon { get; private set; } = default!;

    /// <summary>
    ///     Allow to sting self target
    /// </summary>
    [DataField]
    public bool SelfSting = false;

    /// <summary>
    ///     Event that will be raised on sting
    /// </summary>
    [DataField]
    public object? Event;
}

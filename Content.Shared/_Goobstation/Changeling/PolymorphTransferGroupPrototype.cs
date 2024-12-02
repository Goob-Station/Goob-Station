using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Changeling;

/// <summary>
///     Prototype for stuff that should be transfered on polymorph
/// </summary>
[Prototype("transferGroup")]
public sealed partial class PolymorphTransferGroupPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;


    /// <summary>
    ///     Components that will be transfered
    /// </summary>
    [DataField]
    public ComponentRegistry Components { get; private set; } = default!;
}

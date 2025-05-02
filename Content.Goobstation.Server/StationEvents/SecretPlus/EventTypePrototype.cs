using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.StationEvents.SecretPlus;

/// <summary>
///   Used to specify which events should be possible in the current game director rule.
/// </summary>
[DataDefinition]
[Prototype]
public sealed partial class EventTypePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;
}

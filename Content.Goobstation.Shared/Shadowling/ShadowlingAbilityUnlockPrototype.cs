using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling;

[Prototype]
public sealed partial class ShadowlingAbilityUnlockPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("count")]
    public int UnlockAtThralls;

    [DataField]
    public ComponentRegistry? AddComponents;

    [DataField]
    public ComponentRegistry? RemoveComponents;
}

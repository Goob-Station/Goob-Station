using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Ranching;

[Prototype]
public sealed partial class HappinessPreferencePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}

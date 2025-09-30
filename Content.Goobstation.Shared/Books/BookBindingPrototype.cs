using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Books;

[Prototype("bookBinding")]
public sealed partial class BookBindingLayerPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ResPath RsiPath = new();

    [DataField(required: true)]
    public List<string> AllowedStates = new();

    [DataField(required: true)]
    public string Layer = "";

    [DataField(required: true)]
    public int Priority = 0;
}

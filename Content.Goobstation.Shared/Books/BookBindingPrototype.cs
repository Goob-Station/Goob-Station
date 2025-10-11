using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Books;

[Prototype("bookBinding")]
public sealed partial class BookBindingLayerPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Main RSI path for layer
    /// </summary>
    [DataField(required: true)]
    public ResPath RsiPath = new();

    /// <summary>
    /// Allowed selectable states
    /// </summary>
    [DataField(required: true)]
    public List<string> AllowedStates = new();

    /// <summary>
    /// Target layer map
    /// </summary>
    [DataField(required: true)]
    public string Layer = "";

    [DataField(required: true)]
    public int Priority = 0;
}

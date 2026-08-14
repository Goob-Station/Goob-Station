using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.VoxAudio;

[DataDefinition]
public sealed partial class VoxWord
{
    [DataField]
    public string Word { get; set; } = default!;

    /// <summary>
    /// Override path. Used instead of appending BasePath and .ogg from a VoxVoicePrototype
    /// </summary>
    [DataField]
    public string? Path { get; set; } = default!;
}

[Prototype]
public sealed partial class VoxVoicePrototype : IPrototype
{
    // <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public ResPath BasePath = default!;

    [DataField]
    public List<VoxWord> Words = [];
}
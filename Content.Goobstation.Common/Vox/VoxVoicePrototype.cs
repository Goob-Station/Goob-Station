using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Vox;

[DataDefinition]
public sealed partial class VoxWord
{
    /// <summary>
    /// String of the actual human-readable word.
    /// </summary>
    [DataField]
    public string Word { get; set; } = default!;

    /// <summary>
    /// Override audio path. If specified, used instead of automatically resolving via
    /// <see cref="Word"/> and <see cref="VoxVoicePrototype.ID"/>.
    /// </summary>
    [DataField]
    public ResPath? Path { get; set; } = default!;
}

[Prototype]
public sealed partial class VoxVoicePrototype : IPrototype
{
    // <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Base path for the <see cref="VoxWord"/> used to automatically resolve their path based on their
    /// <see cref="VoxWord.Word"/>, to prevent YAML bloat. This is overridable by <see cref="VoxWord.Path"/>,
    /// and such is optional if all members have explicitly defined paths.
    /// </summary>
    [DataField]
    public string? BasePath = default!;

    /// <summary>
    /// List of all this voice's <see cref="VoxWord"/>s. 
    /// </summary>
    [DataField]
    public List<VoxWord> Words = [];
}

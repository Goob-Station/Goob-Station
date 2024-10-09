using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Changeling;

/// <summary>
///     Used for lings stings
/// </summary>
[Prototype]
public sealed partial class StingPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Localization name of sting
    /// </summary>
    [DataField]
    public string Name = default!;

    /// <summary>
    ///     Sting's UI icon
    /// </summary>
    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/Actions/scream.png"));

    /// <summary>
    ///     Sting components
    /// </summary>
    [DataField(serverOnly: true)]
    public ComponentRegistry Components = new();
}

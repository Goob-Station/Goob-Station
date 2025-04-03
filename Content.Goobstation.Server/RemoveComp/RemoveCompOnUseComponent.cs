using System.ComponentModel.DataAnnotations;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.RemoveComp;

[RegisterComponent]
public sealed partial class RemoveCompOnUseComponent : Component
{
    /// <summary>
    /// Components to remove
    /// </summary>
    [DataField, Required]
    public ComponentRegistry Components = new();

    /// <summary>
    /// Sound to play when removing the components.
    /// </summary>
    public SoundSpecifier SoundOnUse = new SoundPathSpecifier("/Audio/Effects/holy.ogg");

    /// <summary>
    /// Text to display
    /// </summary>
    public LocId PopupText = "remcomp-salvation-text";

    /// <summary>
    /// Play sound?
    /// </summary>
    public bool PlaySoundOnUse = true;

    /// <summary>
    /// Display text?
    /// </summary>
    public bool DisplayTextOnUse = true;
}

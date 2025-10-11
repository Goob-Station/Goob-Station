using Content.Shared.Inventory;
using Content.Shared.Speech;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Loudspeaker.Components;

/// <summary>
/// Used for items (or entities) that have loudspeaker capabilities.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LoudspeakerComponent : Component
{
    /// <summary>
    /// Should it work in your hands?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool WorksInHand;

    /// <summary>
    /// Can it be toggled on/off?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanToggle;

    /// <summary>
    /// Is the loudspeaker active?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;

    /// <summary>
    /// Should it affect regular speaking?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AffectChat;

    /// <summary>
    /// Should it affect speaking via radio?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AffectRadio;

    /// <summary>
    /// How big should the new text font be?
    /// </summary>
    [DataField, AutoNetworkedField]
    public int FontSize = 18;

    /// <summary>
    /// The slot it should take up to work.
    /// </summary>
    [DataField]
    public SlotFlags RequiredSlot = SlotFlags.EARS;

    /// <summary>
    /// The sound it should play for the user when toggling.
    /// </summary>
    [DataField]
    public SoundPathSpecifier ToggleSound = new("/Audio/Items/pen_click.ogg");

    /// <summary>
    /// The sounds the user will make when speaking.
    /// </summary>
    [DataField]
    public ProtoId<SpeechSoundsPrototype>? SpeechSounds;
}

using Content.Shared.Inventory;

namespace Content.Shared.Chat;
/// <summary>
/// Raised whenever a chat message is sent, contains the font id, font size, font colour and the sender's EntityUid. Goobstation feature
/// </summary>
public sealed class TransformSpeakerFontEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;
    public EntityUid Sender;
    public string? FontId;
    public int? FontSize;
    public Color? Color;

    public TransformSpeakerFontEvent(EntityUid sender, string? fontid = null, int? fontsize = null, Color? color = null)
    {
        Sender = sender;
        FontId = fontid;
        FontSize = fontsize;
        Color = color;
    }
}

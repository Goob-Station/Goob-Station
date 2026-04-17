

using Content.Shared.Chat;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.SpeakFont;

public sealed partial class SpeakFontSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SpeakFontComponent, InventoryRelayedEvent<TransformSpeakerFontEvent>>(OnFontEvent);
    }

    private void OnFontEvent(Entity<SpeakFontComponent> entity, ref InventoryRelayedEvent<TransformSpeakerFontEvent> args)
    {
        args.Args.FontId = entity.Comp.FontId;
        args.Args.FontSize = entity.Comp.FontSize;
        args.Args.Color = entity.Comp.Color;
    }
}
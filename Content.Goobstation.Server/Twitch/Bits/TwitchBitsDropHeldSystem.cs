using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsDropHeldSystem : EntitySystem, ITwitchBitsAction
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;

    public string Id => "drop-held";
    public string DisplayName => "Drop Held Item";
    public string DisplayDescription => "Force the streamer to drop the item in their active hand.";
    public string Category => "Inventory";
    public string Sku => "ss14-drop-held";

    public override void Initialize()
    {
        base.Initialize();
        _twitchBits.RegisterAction(this);
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        if (!TryComp<HandsComponent>(target, out var hands) ||
            _hands.GetActiveItem((target, hands)) == null)
        {
            return TwitchBitsActionValidity.Invalid("The streamer is not holding anything in their active hand.");
        }

        var activeHand = _hands.GetActiveHand((target, hands));
        if (activeHand == null || !_hands.CanDropHeld(target, activeHand, checkActionBlocker: false))
            return TwitchBitsActionValidity.Invalid("The item in the streamer's active hand cannot be dropped.");

        return TwitchBitsActionValidity.Valid;
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        return TryComp<HandsComponent>(target, out var hands) &&
               _hands.TryDrop(
                   (target, hands),
                   checkActionBlocker: false,
                   doDropInteraction: false);
    }
}

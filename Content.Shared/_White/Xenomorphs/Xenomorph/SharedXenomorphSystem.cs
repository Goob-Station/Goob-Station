using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Tag;

namespace Content.Shared._White.Xenomorphs.Xenomorph;

public abstract class SharedXenomorphSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    [ValidatePrototypeId<TagPrototype>]
    private const string XenomorphItemTag = "XenomorphItem";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphComponent, PickupAttemptEvent>(OnPickup);
    }

    private void OnPickup(EntityUid uid, XenomorphComponent component, PickupAttemptEvent args)
    {
        if (_tag.HasTag(args.Item, XenomorphItemTag))
            return;

        _popup.PopupClient(Loc.GetString("xenomorph-pickup-item-fail"), args.Item, uid);
        args.Cancel();
    }
}

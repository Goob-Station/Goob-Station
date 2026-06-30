using Content.Shared.Inventory;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Pirate.Client.Newbies;

public sealed class NoviceMarkIconSystem : EntitySystem
{
    private const string NeckSlot = "neck";

    private static readonly EntProtoId NoviceMarkPrototype = "ClothingNeckNoviceMark";

    private static readonly StatusIconData NoviceMarkIcon = new()
    {
        Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Pirate/Interface/Misc/novice_status.rsi"), "novice"),
        Priority = 0,
        LocationPreference = StatusIconLocationPreference.Right,
    };

    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusIconComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(EntityUid uid, StatusIconComponent component, ref GetStatusIconsEvent args)
    {
        if (!TryComp<InventoryComponent>(uid, out var inventory) ||
            !_inventory.TryGetSlotEntity(uid, NeckSlot, out var neckItem, inventory))
            return;

        if (!TryComp<MetaDataComponent>(neckItem, out var meta) ||
            meta.EntityPrototype?.ID != NoviceMarkPrototype.Id)
            return;

        args.StatusIcons.Add(NoviceMarkIcon);
    }
}

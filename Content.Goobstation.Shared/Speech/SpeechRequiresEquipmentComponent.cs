// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Speech;

public sealed partial class SpeechRequiresEquipmentSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeechRequiresEquipmentComponent, SpeakAttemptEvent>(OnSpeechAttempt);
    }

    public void OnSpeechAttempt(Entity<SpeechRequiresEquipmentComponent> ent, ref SpeakAttemptEvent args)
    {
        foreach (var (slot, whitelist) in ent.Comp.Equipment)
        {
            if (!_inventory.TryGetSlotEntity(ent, slot, out var item)
                || _whitelist.IsWhitelistFail(whitelist, item.Value))
            {
                if (ent.Comp.FailMessage != null)
                    _popup.PopupClient(Loc.GetString(ent.Comp.FailMessage), ent, ent);

                args.Cancel();
                return;
            }
        }
    }
}

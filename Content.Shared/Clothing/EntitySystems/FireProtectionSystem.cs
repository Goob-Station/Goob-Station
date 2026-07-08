// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Flammability;
using Content.Shared.Armor;
using Content.Shared.Atmos;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;

namespace Content.Shared.Clothing.EntitySystems;

/// <summary>
/// Handles reducing fire damage when wearing clothing with <see cref="FireProtectionComponent"/>.
/// </summary>
public sealed class FireProtectionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FireProtectionComponent, InventoryRelayedEvent<GetFireProtectionEvent>>(OnGetProtection);
        SubscribeLocalEvent<FireProtectionComponent, ArmorExamineEvent>(OnArmorExamine);
    }

    private void OnGetProtection(Entity<FireProtectionComponent> ent, ref InventoryRelayedEvent<GetFireProtectionEvent> args)
    {
        // goob edit - VERY flammable component (trademark)
        if (HasComp<VeryFlammableComponent>(ent))
            return;

        args.Args.Reduce(ent.Comp.Reduction);
    }

    private void OnArmorExamine(Entity<FireProtectionComponent> ent, ref ArmorExamineEvent args)
    {
        var value = MathF.Round(ent.Comp.Reduction * 100, 1);

        if (value == 0)
            return;

        args.Msg.PushNewline();
        args.Msg.AddMarkupOrThrow(Loc.GetString(ent.Comp.ExamineMessage, ("value", value)));
    }
}

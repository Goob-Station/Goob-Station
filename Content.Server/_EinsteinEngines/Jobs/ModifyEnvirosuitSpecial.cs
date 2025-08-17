// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Skubman <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Roles;
using Content.Shared._EinsteinEngines.SelfExtinguisher;
using JetBrains.Annotations;

namespace Content.Server._EinsteinEngines.Jobs;

[UsedImplicitly]
public sealed partial class ModifyEnvirosuitSpecial : JobSpecial
{
    // <summary>
    //   The new charges of the envirosuit's self-extinguisher.
    // </summary>
    [DataField(required: true)]
    public int Charges { get; private set; }

    [ValidatePrototypeId<SpeciesPrototype>]
    private const string Species = "Plasmaman";

    private const string Slot = "jumpsuit";

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        if (!entMan.TryGetComponent<HumanoidAppearanceComponent>(mob, out var appearance) ||
            appearance.Species != Species ||
            !entMan.System<InventorySystem>().TryGetSlotEntity(mob, Slot, out var jumpsuit) ||
            jumpsuit is not { } envirosuit ||
            !entMan.TryGetComponent<SelfExtinguisherComponent>(envirosuit, out var selfExtinguisher))
            return;

        entMan.System<SharedSelfExtinguisherSystem>().SetCharges(envirosuit, Charges, selfExtinguisher);
    }
}

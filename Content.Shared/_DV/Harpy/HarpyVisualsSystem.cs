// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory.Events;
using Content.Shared.Tag;
using Content.Shared.Humanoid;
using Content.Shared._NF.Clothing.Components; // Frontier

namespace Content.Shared._DV.Harpy;

public sealed class HarpyVisualsSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidSystem = default!;

    [ValidatePrototypeId<TagPrototype>]
    private const string HarpyWingsTag = "HidesHarpyWings";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HarpySingerComponent, DidEquipEvent>(OnDidEquipEvent);
        SubscribeLocalEvent<HarpySingerComponent, DidUnequipEvent>(OnDidUnequipEvent);
    }

    private void OnDidEquipEvent(EntityUid uid, HarpySingerComponent component, DidEquipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag && HasComp<HarpyHideWingsComponent>(args.Equipment, HarpyWingsTag))
        {
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.RArm, false);
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.RArmExtension, false);
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.Tail, false);
        }
    }

    private void OnDidUnequipEvent(EntityUid uid, HarpySingerComponent component, DidUnequipEvent args)
    {
        if (args.Slot == "outerClothing" && _tagSystem.HasTag && HasComp<HarpyHideWingsComponent>(args.Equipment, HarpyWingsTag))
        {
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.RArm, true);
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.RArmExtension, true);
            _humanoidSystem.SetLayerVisibility(uid, HumanoidVisualLayers.Tail, true);
        }
    }
}

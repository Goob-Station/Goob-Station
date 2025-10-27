// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.EntitySystems;
using Content.Server.Stack;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Stacks;

namespace Content.Goobstation.Server.MaterialEnergy
{
    public sealed class MaterialEnergySystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly BatterySystem _batterySystem = default!;
        [Dependency] private readonly StackSystem _stack = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MaterialEnergyComponent, InteractUsingEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, MaterialEnergyComponent component, InteractUsingEvent args)
        {
            if (component.MaterialWhiteList == null)
                return;

            _entityManager.TryGetComponent<PhysicalCompositionComponent>(args.Used, out var _composition);
            if (_composition == null)
                return;

            _entityManager.TryGetComponent<StackComponent>(args.Used, out var materialStack);
            if (materialStack == null)
                return;

            foreach (var fueltype in component.MaterialWhiteList)
            {
                if (_composition.MaterialComposition.ContainsKey(fueltype))
                    AddBatteryCharge(
                        uid,
                        args.Used,
                        _composition.MaterialComposition[fueltype],
                        materialStack.Count);
            }
        }

        private void AddBatteryCharge(
            EntityUid cutter,
            EntityUid _material,
            int materialPerSheet,
            int sheetsInStack)
        {
            var chargeDiff = _batterySystem.GetChargeDifference(cutter);
            if (chargeDiff == 0)
                return;

            var totalMaterial = materialPerSheet * sheetsInStack;
            var materialLeft = totalMaterial - chargeDiff;
            var chargeToAdd = 0;

            if (materialLeft == 0)
            {
                chargeToAdd = totalMaterial;
            }
            else if (materialLeft > 0)
            {
                chargeToAdd = (totalMaterial - materialLeft);
            }
            else
            {
                chargeToAdd = Math.Abs(Math.Abs(materialLeft) - chargeDiff);
            }

            _batterySystem.AddCharge(cutter, chargeToAdd);

            var toDel = _stack.Split(
                (EntityUid) _material,
                chargeToAdd / materialPerSheet,
                Transform(_material).Coordinates);
            QueueDel(toDel);
        }
    }
}
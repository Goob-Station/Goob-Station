using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Content.Server.Stack;
using Content.Server.Power.EntitySystems;

namespace Content.Server._Goobstation.MaterialEnergy
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

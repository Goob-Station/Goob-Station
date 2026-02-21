using Content.Goobstation.Shared.PlasmaCutterFuel;
using Content.Server.Power.EntitySystems;
using Content.Server.Stack;
using Content.Shared.Interaction;
using Content.Shared.Stacks;

namespace Content.Goobstation.Server.PlasmaCutterEnergy
{
    public sealed class PlasmaCutterEnergySystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly BatterySystem _batterySystem = default!;
        [Dependency] private readonly StackSystem _stack = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PlasmaCutterEnergyComponent, InteractUsingEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, PlasmaCutterEnergyComponent component, InteractUsingEvent args)
        {
            _entityManager.TryGetComponent<PlasmaCutterFuelComponent>(args.Used, out var fuel);
            if (fuel == null)
                return;

            _entityManager.TryGetComponent<StackComponent>(args.Used, out var materialStack);
            if (materialStack == null)
                return;

            AddBatteryCharge(
                uid,
                args.Used,
                fuel.EnergyPerSheet,
                materialStack.Count);
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

            var sheetsToConsume = (int) Math.Ceiling((double) chargeToAdd / materialPerSheet);

            var toDel = _stack.Split(
                (EntityUid) _material,
                sheetsToConsume,
                Transform(_material).Coordinates);
            QueueDel(toDel);
        }
    }
}
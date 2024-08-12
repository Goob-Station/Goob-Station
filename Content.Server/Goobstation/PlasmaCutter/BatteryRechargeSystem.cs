using Content.Server.Materials;
using Content.Shared.Materials;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.Components;

namespace Content.Server.Goobstation.Plasmacutter
{
    public sealed class BatteryRechargeSystem : EntitySystem
    {
        [Dependency] private readonly MaterialStorageSystem _materialStorage = default!;
        [Dependency] private readonly BatterySystem _batterySystem = default!;


        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MaterialStorageComponent, MaterialEntityInsertedEvent>(OnMaterialAmountChanged);
            SubscribeLocalEvent<BatteryRechargeComponent, ChargeChangedEvent>(OnChargeChanged);
        }

        private void OnMaterialAmountChanged(EntityUid uid, MaterialStorageComponent component, MaterialEntityInsertedEvent args)
        {
            if (component.MaterialWhiteList != null)
                foreach (var fuelType in component.MaterialWhiteList)
                {
                    FuelAddCharge(uid, fuelType);
                }
        }

        private void OnChargeChanged(EntityUid uid, BatteryRechargeComponent component, ChargeChangedEvent args)
        {
            ChangeStorageLimit(uid, component.StorageMaxCapacity);
        }

        private void ChangeStorageLimit(
            EntityUid uid,
            int value,
            BatteryComponent? battery = null)
        {
            if (!Resolve(uid, ref battery))
                return;
            if (battery.CurrentCharge == battery.MaxCharge)
                value = 0;
            _materialStorage.TryChangeStorageLimit(uid, value);
        }

        private void FuelAddCharge(
            EntityUid uid,
            string fuelType,
            BatteryRechargeComponent? recharge = null)
        {
            if (!Resolve(uid, ref recharge))
                return;

            var availableMaterial = _materialStorage.GetMaterialAmount(uid, fuelType);
            var chargePerMaterial = availableMaterial * recharge.Multiplier;

            if (_materialStorage.TryChangeMaterialAmount(uid, fuelType, -availableMaterial))
            {
                _batterySystem.TryAddCharge(uid, chargePerMaterial);
            }
        }
    }
}
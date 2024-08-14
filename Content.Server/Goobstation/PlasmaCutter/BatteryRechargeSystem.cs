using Content.Shared.Materials;
using Content.Shared.Interaction.Events;
using Content.Server.Hands.Systems;
using Content.Server.Materials;
using Content.Server.Power.EntitySystems;
using Content.Server.Power.Components;

namespace Content.Server.Goobstation.Plasmacutter
{
    public sealed class BatteryRechargeSystem : EntitySystem
    {
        [Dependency] private readonly MaterialStorageSystem _materialStorage = default!;
        [Dependency] private readonly BatterySystem _batterySystem = default!;
        [Dependency] private readonly HandsSystem _hands = default!;

        private EntityUid playerUid;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MaterialStorageComponent, ContactInteractionEvent>(OnInteract);
            SubscribeLocalEvent<MaterialStorageComponent, MaterialEntityInsertedEvent>(OnMaterialAmountChanged);
            SubscribeLocalEvent<BatteryRechargeComponent, ChargeChangedEvent>(OnChargeChanged);
        }

        private void OnInteract(EntityUid uid, MaterialStorageComponent component, ContactInteractionEvent args)
        {
            playerUid = args.Other;
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

            if (_materialStorage.TryChangeMaterialAmount(uid, fuelType, -availableMaterial))
            {
                // this is shit. this shit works.
                var spawnAmount = _batterySystem.GetChargeDifference(uid) - availableMaterial;
                if (spawnAmount < 0)
                {
                    spawnAmount = Math.Abs(spawnAmount);
                }
                else {
                    spawnAmount = 0;
                }

                var ent = _materialStorage.SpawnMultipleFromMaterial(spawnAmount, fuelType, Transform(uid).Coordinates, out var overflow);

                foreach (var entUid in ent)
                {
                    _hands.TryForcePickupAnyHand(playerUid, entUid);
                }
                
                _batterySystem.AddCharge(uid, availableMaterial);
            }
        }
    }
}
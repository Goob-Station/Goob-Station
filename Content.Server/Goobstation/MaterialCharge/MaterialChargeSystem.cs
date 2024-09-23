using Content.Shared.Interaction;
using Content.Shared.Materials;
using Content.Shared.Stacks;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Server.Stack;

namespace Content.Server.Goobstation.MaterialCharge
{
    public sealed class MaterialChargeSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly StackSystem _stack = default!;
        [Dependency] private readonly SharedChargesSystem _chargeSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MaterialChargeComponent, InteractUsingEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, MaterialChargeComponent component, InteractUsingEvent args)
        {
            if (component.MaterialWhiteList == null)
                return;

            _entityManager.TryGetComponent<LimitedChargesComponent>(uid, out var charges);
            if (charges == null)
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
                    if (_composition.MaterialComposition[fueltype] != 100)
                        continue;
                    AddCharge(
                        uid, 
                        args.Used, 
                        charges, 
                        materialStack.Count);
            }
        }

        private void AddCharge(
            EntityUid uid, 
            EntityUid _material,
            LimitedChargesComponent charges,
            int sheetsInStack)
        {
            var chargeDiff = charges.MaxCharges - charges.Charges;

            var materialLeft = sheetsInStack - chargeDiff;
            var chargeToAdd = 0;

            if (materialLeft == 0)
            {
                chargeToAdd = sheetsInStack;
            }
            else if (materialLeft > 0)
            {
                chargeToAdd = (sheetsInStack - materialLeft);
            }
            else
            {
                chargeToAdd = Math.Abs(Math.Abs(materialLeft) - chargeDiff);
            }

            _chargeSystem.AddCharges(uid, chargeToAdd, charges);

            var toDel = _stack.Split(
                (EntityUid) _material, 
                chargeToAdd, 
                Transform(_material).Coordinates);
            QueueDel(toDel);
        }
    }
}
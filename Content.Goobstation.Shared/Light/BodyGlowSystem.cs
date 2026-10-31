using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Goobstation.Shared.Light
{
    public sealed partial class BodyGlowSystem : EntitySystem
    {
        [Dependency] private InventorySystem _inventory = default!;
        [Dependency] private SharedPointLightSystem _lights = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BodyGlowComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<BodyGlowComponent, DidEquipEvent>(OnEquipped);
            SubscribeLocalEvent<BodyGlowComponent, DidUnequipEvent>(OnUnequipped);
        }

        private void OnMapInit(Entity<BodyGlowComponent> ent, ref MapInitEvent args)
        {
            UpdateGlow(ent);
        }

        private void OnEquipped(Entity<BodyGlowComponent> ent, ref DidEquipEvent args)
        {
            UpdateGlow(ent);
        }

        private void OnUnequipped(Entity<BodyGlowComponent> ent, ref DidUnequipEvent args)
        {
            UpdateGlow(ent);
        }

        private void UpdateGlow(Entity<BodyGlowComponent> ent)
        {
            if (!_lights.TryGetLight(ent, out var light))
                return;

            var reduction = 0f;
            if (TryComp(ent.Owner, out InventoryComponent? inventory))
            {
                var slots = _inventory.GetSlotEnumerator((ent.Owner, inventory));
                while (slots.NextItem(out var item))
                {
                    if (TryComp<GlowShieldingComponent>(item, out var shield))
                        reduction += shield.Reduction;
                }
            }

            var factor = Math.Clamp(1f - reduction, 0f, 1f);
            ent.Comp.Factor = factor;

            if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
                _lights.SetColor(ent, humanoid.SkinColor, light);

            if (factor <= 0f)
            {
                _lights.SetEnabled(ent, false, light);
                return;
            }

            _lights.SetRadius(ent, ent.Comp.BaseRadius * factor, light);
            _lights.SetEnergy(ent, ent.Comp.BaseEnergy * factor, light);
            _lights.SetEnabled(ent, true, light);
        }
    }
}

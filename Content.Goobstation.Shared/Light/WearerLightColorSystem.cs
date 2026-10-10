using Content.Shared.Clothing;
using Content.Shared.Humanoid;
using Content.Shared.Toggleable;

namespace Content.Goobstation.Shared.Light
{
    public sealed class WearerLightColorSystem : EntitySystem
    {
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedPointLightSystem _lights = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<WearerLightColorComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<WearerLightColorComponent, ClothingGotEquippedEvent>(OnEquipped);
            SubscribeLocalEvent<WearerLightColorComponent, ClothingGotUnequippedEvent>(OnUnequipped);
        }

        private void OnMapInit(Entity<WearerLightColorComponent> ent, ref MapInitEvent args)
        {
            if (_lights.TryGetLight(ent, out var light))
                ent.Comp.DefaultColor = light.Color;
        }

        private void OnEquipped(Entity<WearerLightColorComponent> ent, ref ClothingGotEquippedEvent args)
        {
            var color = ent.Comp.DefaultColor;

            if (TryComp<HumanoidAppearanceComponent>(args.Wearer, out var humanoid)
                && (ent.Comp.Species == null || humanoid.Species == ent.Comp.Species))
            {
                color = humanoid.SkinColor;
            }

            _appearance.SetData(ent, ToggleableVisuals.Color, color);
        }

        private void OnUnequipped(Entity<WearerLightColorComponent> ent, ref ClothingGotUnequippedEvent args)
        {
            _appearance.SetData(ent, ToggleableVisuals.Color, ent.Comp.DefaultColor);
        }
    }
}

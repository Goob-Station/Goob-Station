using Content.Shared.Clothing;
using Content.Shared.Humanoid;
using Content.Shared.Toggleable;

namespace Content.Goobstation.Shared.Light
{
    public sealed partial class WearerLightColorSystem : EntitySystem
    {
        [Dependency] private SharedAppearanceSystem _appearance = default!;
        [Dependency] private SharedPointLightSystem _lights = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<WearerLightColorComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<WearerLightColorComponent, ClothingGotEquippedEvent>(OnEquipped);
            SubscribeLocalEvent<WearerLightColorComponent, ClothingGotUnequippedEvent>(OnUnequipped);
            SubscribeLocalEvent<WearerLightColorComponent, AttemptPointLightToggleEvent>(OnLightAttempt);
        }

        private void OnMapInit(Entity<WearerLightColorComponent> ent, ref MapInitEvent args)
        {
            if (_lights.TryGetLight(ent, out var light))
                ent.Comp.DefaultColor = light.Color;
        }

        private void OnEquipped(Entity<WearerLightColorComponent> ent, ref ClothingGotEquippedEvent args)
        {
            Color color;
            bool suppressed;

            if (TryComp<HumanoidAppearanceComponent>(args.Wearer, out var humanoid)
                && (ent.Comp.Species == null || humanoid.Species == ent.Comp.Species))
            {
                color = humanoid.SkinColor;
                suppressed = false;
            }
            else
            {
                color = ent.Comp.DefaultColor;
                suppressed = ent.Comp.Species != null;
            }

            SetSuppressed(ent, suppressed);
            _appearance.SetData(ent, ToggleableVisuals.Color, color);
            SyncLight(ent);
        }

        private void OnUnequipped(Entity<WearerLightColorComponent> ent, ref ClothingGotUnequippedEvent args)
        {
            SetSuppressed(ent, false);
            _appearance.SetData(ent, ToggleableVisuals.Color, ent.Comp.DefaultColor);
            SyncLight(ent);
        }

        private void OnLightAttempt(Entity<WearerLightColorComponent> ent, ref AttemptPointLightToggleEvent args)
        {
            if (args.Enabled && ent.Comp.LightSuppressed)
                args.Cancelled = true;
        }

        private void SetSuppressed(Entity<WearerLightColorComponent> ent, bool suppressed)
        {
            if (ent.Comp.LightSuppressed == suppressed)
                return;

            ent.Comp.LightSuppressed = suppressed;
            Dirty(ent);
        }

        private void SyncLight(Entity<WearerLightColorComponent> ent)
        {
            if (!_appearance.TryGetData<bool>(ent, ToggleableVisuals.Enabled, out var enabled) || !enabled)
                return;

            if (!_lights.TryGetLight(ent, out var light))
                return;

            _lights.SetEnabled(ent, !ent.Comp.LightSuppressed, light);
        }
    }
}

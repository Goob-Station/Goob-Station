using System.Linq;
using Robust.Client.GameObjects;
using static Robust.Client.GameObjects.SpriteComponent;
using Content.Client.Kitchen.Components;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Kitchen.Components;
using Content.Shared.Nyanotrasen.Kitchen.Components;

namespace Content.Client.Kitchen.Visualizers
{
    public sealed class DeepFriedVisualizerSystem : VisualizerSystem<DeepFriedComponent>
    {
        [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
        private readonly static string ShaderName = "Crispy";

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DeepFriedComponent, HeldVisualsUpdatedEvent>(OnHeldVisualsUpdated);
            SubscribeLocalEvent<DeepFriedComponent, EquipmentVisualsUpdatedEvent>(OnEquipmentVisualsUpdated);
        }

        protected override void OnAppearanceChange(EntityUid uid, DeepFriedComponent component, ref AppearanceChangeEvent args)
        {
            if (args.Sprite == null)
                return;

            if (!_appearanceSystem.TryGetData(uid, DeepFriedVisuals.Fried, out bool isFried))
                return;

            for (var i = 0; i < args.Sprite.AllLayers.Count(); ++i)
                args.Sprite.LayerSetShader(i, ShaderName);
        }

        private void OnHeldVisualsUpdated(EntityUid uid, DeepFriedComponent component, HeldVisualsUpdatedEvent args)
        {
            if (args.RevealedLayers.Count == 0)
            {
                return;
            }

            if (!TryComp(args.User, out SpriteComponent? sprite))
                return;

            foreach (var key in args.RevealedLayers)
            {
                if (!sprite.LayerMapTryGet(key, out var index) || sprite[index] is not Layer layer)
                    continue;

                sprite.LayerSetShader(index, ShaderName);
            }
        }

        private void OnEquipmentVisualsUpdated(EntityUid uid, DeepFriedComponent component, EquipmentVisualsUpdatedEvent args)
        {
            if (args.RevealedLayers.Count == 0)
            {
                return;
            }

            if (!TryComp(args.Equipee, out SpriteComponent? sprite))
                return;

            foreach (var key in args.RevealedLayers)
            {
                if (!sprite.LayerMapTryGet(key, out var index) || sprite[index] is not Layer layer)
                    continue;

                sprite.LayerSetShader(index, ShaderName);
            }
        }
    }
}

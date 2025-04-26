using Robust.Client.GameObjects;
using Content.Client.Chemistry.Visualizers;
using Content.Client.Kitchen.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Kitchen.Components;
using Content.Shared.Nyanotrasen.Kitchen.Components;

namespace Content.Client.Kitchen.Visualizers
{
    public sealed class DeepFryerVisualizerSystem : VisualizerSystem<DeepFryerComponent>
    {
        [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;

        protected override void OnAppearanceChange(EntityUid uid, DeepFryerComponent component, ref AppearanceChangeEvent args)
        {
            if (!_appearanceSystem.TryGetData(uid, DeepFryerVisuals.Bubbling, out bool isBubbling) ||
                !TryComp<SolutionContainerVisualsComponent>(uid, out var scvComponent))
            {
                return;
            }

            scvComponent.FillBaseName = isBubbling ? "on-" : "off-";
        }
    }
}

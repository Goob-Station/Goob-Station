using Content.Client.DamageState;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Mobs;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Xenobiology;

/// <summary>
/// This handles visual changes in mobs which can transition growth states.
/// </summary>
public sealed class MobGrowthVisualizerSystem : VisualizerSystem<MobGrowthComponent>
{
    //I have a feeling this may need some protective functions.
    protected override void OnAppearanceChange(EntityUid uid, MobGrowthComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<string>(uid, GrowthStateVisuals.Stage, out var state, args.Component))
        {
            args.Sprite.LayerSetState(DamageStateVisualLayers.Base, state);
        }
    }
}

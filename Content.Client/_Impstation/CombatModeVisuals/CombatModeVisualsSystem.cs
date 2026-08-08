using Content.Client.DamageState;
using Content.Shared._Impstation.CombatModeVisuals;
using Content.Shared.CombatMode;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Impstation.CombatModeVisuals;

public sealed partial class CombatModeVisualsSystem : SharedCombatModeVisualsSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatModeVisualsComponent, ToggleCombatActionEvent>(OnToggleCombat);
        SubscribeLocalEvent<CombatModeVisualsComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<CombatModeVisualsComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnToggleCombat(Entity<CombatModeVisualsComponent> ent, ref ToggleCombatActionEvent args)
    {
        ChangeAppearance(ent);
    }

    private void OnMobStateChanged(Entity<CombatModeVisualsComponent> ent, ref MobStateChangedEvent args)
    {
        ChangeAppearance(ent);
    }

    private void ChangeAppearance(EntityUid ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        _appearance.OnChangeData(ent, sprite);
    }

    private void OnAppearanceChanged(Entity<CombatModeVisualsComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        if (!TryComp<CombatModeComponent>(ent, out var combat))
            return;
        if (!args.Sprite.LayerMapTryGet(CombatModeVisualsVisuals.Combat, out var combatIdx)
            || !args.Sprite.LayerMapTryGet(DamageStateVisualLayers.Base, out var baseIdx))
            return;

        // make sure we can sync the frames
        if (!args.Sprite.TryGetLayer(combatIdx, out var combatLayer)
            || !args.Sprite.TryGetLayer(baseIdx, out var baseLayer))
            return;

        // turn on combat visuals if the mob is alive and in combat mode. otherwise turn them off
        args.Sprite.LayerSetVisible(combatIdx, _mobState.IsAlive(ent) && combat.IsInCombatMode);

        // handle hiding/unhiding the base layer if applicable
        if (ent.Comp.HideBaseLayer && _mobState.IsAlive(ent))
            args.Sprite.LayerSetVisible(baseIdx, !combat.IsInCombatMode);
        else if (ent.Comp.HideBaseLayer)
            args.Sprite.LayerSetVisible(baseIdx, true);

        // then sync them to the base animation
        if (combatLayer.AutoAnimated)
        {
            combatLayer.SetAnimationTime(baseLayer.AnimationTime);
            combatLayer.AnimationFrame = baseLayer.AnimationFrame;
            combatLayer.AnimationTimeLeft = baseLayer.AnimationTimeLeft;
        }
    }
}

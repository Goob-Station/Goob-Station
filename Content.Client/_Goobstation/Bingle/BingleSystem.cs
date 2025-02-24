using Robust.Client.GameObjects;
using Content.Shared._Goobstation.Bingle;
using Content.Shared.CombatMode;

namespace Content.Client._Goobstation.Bingle;

/// <summary>
///   handles the upgrade apperance from normal bingle to upgraded bingle
/// </summary>
public sealed class BingleSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<BingleUpgradeEntityMessage>(OnUpgradeChange);
        SubscribeLocalEvent<BingleComponent, ToggleCombatActionEvent>(OnCombatToggle);
    }

    //make eyse glow red when combat mode engaged.
    private void OnCombatToggle(EntityUid uid, BingleComponent component, ToggleCombatActionEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        if (!TryComp<CombatModeComponent>(uid, out var combat))
            return;
        if (!sprite.LayerMapTryGet(BingleVisual.Combat, out var layer))
            return;

        sprite.LayerSetVisible(layer, combat.IsInCombatMode);
    }

    private void OnUpgradeChange(BingleUpgradeEntityMessage args)
    {
        var uid = GetEntity(args.Bingle);

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        if (!sprite.LayerMapTryGet(BingleVisual.Upgraded, out var layer))
            return;

        sprite.LayerSetVisible(layer, true);
    }
}

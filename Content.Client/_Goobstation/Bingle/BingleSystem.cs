using Robust.Client.GameObjects;
using Content.Shared._Goobstation.Bingle;
using Content.Shared.CombatMode;

namespace Content.Client._Goobstation.Bingle;

/// <summary>
///   handles the apperance bingles
/// </summary>
public sealed class BingleSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BingleComponent, ToggleCombatActionEvent>(OnCombatToggle);
    }

    /// <summary>
    /// make eyse glow red when combat mode engaged.
    /// </summary>
    private void OnCombatToggle(EntityUid uid, BingleComponent component, ToggleCombatActionEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        if (!TryComp<CombatModeComponent>(uid, out var combat))
            return;
        if (!sprite.LayerMapTryGet(BingleVisual.Combat, out var layer))
            return;

        sprite.LayerSetVisible(layer, combat.IsInCombatMode);
        _appearance.OnChangeData(uid, sprite);
    }
}

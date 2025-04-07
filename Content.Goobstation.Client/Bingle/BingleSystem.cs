// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 unknown <Administrator@DESKTOP-PMRIVVA.kommune.indresogn.no>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bingle;
using Content.Shared.CombatMode;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Goobstation.Client.Bingle;

/// <summary>
///   Handles the appearance of bingles.
/// </summary>
public sealed class BingleSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BingleComponent, ToggleCombatActionEvent>(OnCombatToggle);
        SubscribeLocalEvent<BingleComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    /// <summary>
    /// Makes the eyes glow red when combat mode is engaged.
    /// </summary>
    private void OnCombatToggle(EntityUid uid, BingleComponent component, ToggleCombatActionEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;
        _appearance.OnChangeData(uid, sprite);
    }

    public void OnAppearanceChange(EntityUid uid, BingleComponent component, ref AppearanceChangeEvent args)
    {
        var sprite = args.Sprite;
        if (sprite == null)
            return;
        if (!TryComp<CombatModeComponent>(uid, out var combat))
            return;
        if (!sprite.LayerMapTryGet(BingleVisual.Combat, out var layer))
            return;
        sprite.LayerSetVisible(layer, combat.IsInCombatMode);
    }
}
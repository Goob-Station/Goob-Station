// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.AlertLevel;
using Robust.Client.GameObjects;

namespace Content.Client.AlertLevel;

public sealed class AlertLevelDisplaySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlertLevelDisplayComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, AlertLevelDisplayComponent alertLevelDisplay, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
        {
            return;
        }
        var layer = args.Sprite.LayerMapReserveBlank(AlertLevelDisplay.Layer);

        if (args.AppearanceData.TryGetValue(AlertLevelDisplay.Powered, out var poweredObject))
        {
            args.Sprite.LayerSetVisible(layer, poweredObject is true);
        }

        if (!args.AppearanceData.TryGetValue(AlertLevelDisplay.CurrentLevel, out var level))
        {
            args.Sprite.LayerSetState(layer, alertLevelDisplay.AlertVisuals.Values.First());
            return;
        }

        if (alertLevelDisplay.AlertVisuals.TryGetValue((string) level, out var visual))
        {
            args.Sprite.LayerSetState(layer, visual);
        }
        else
        {
            args.Sprite.LayerSetState(layer, alertLevelDisplay.AlertVisuals.Values.First());
        }
    }
}
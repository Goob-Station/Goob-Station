// SPDX-FileCopyrightText: 2023 Bixkitts <72874643+Bixkitts@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.GameObjects;
using Content.Shared.Smoking;

namespace Content.Client.Smoking;

public sealed class BurnStateVisualizerSystem : VisualizerSystem<BurnStateVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, BurnStateVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        if (!args.AppearanceData.TryGetValue(SmokingVisuals.Smoking, out var burnState))
            return;

        var state = burnState switch
        {
            SmokableState.Lit => component.LitIcon,
            SmokableState.Burnt => component.BurntIcon,
            _ => component.UnlitIcon
        };

        args.Sprite.LayerSetState(0, state);
    }
}

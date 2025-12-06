// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
        if (args.Sprite == null
            || !AppearanceSystem.TryGetData<string>(uid, GrowthStateVisuals.Sprite, out var rsi, args.Component))
            return;

        args.Sprite.LayerSetRSI(DamageStateVisualLayers.Base, rsi);
    }
}

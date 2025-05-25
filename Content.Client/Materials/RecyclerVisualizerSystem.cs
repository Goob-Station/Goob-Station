// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Conveyor;
using Content.Shared.Materials;
using Robust.Client.GameObjects;

namespace Content.Client.Materials;

public sealed class RecyclerVisualizerSystem : VisualizerSystem<RecyclerVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, RecyclerVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !args.Sprite.LayerMapTryGet(RecyclerVisualLayers.Main, out var layer))
            return;

        AppearanceSystem.TryGetData<ConveyorState>(uid, ConveyorVisuals.State, out var running);
        AppearanceSystem.TryGetData<bool>(uid, RecyclerVisuals.Bloody, out var bloody);
        AppearanceSystem.TryGetData<bool>(uid, RecyclerVisuals.Broken, out var broken);

        var activityState = running == ConveyorState.Off ? 0 : 1;
        if (broken) //breakage overrides activity
            activityState = 2;

        var bloodyKey = bloody ? component.BloodyKey : string.Empty;

        var state = $"{component.BaseKey}{activityState}{bloodyKey}";
        args.Sprite.LayerSetState(layer, state);
    }
}
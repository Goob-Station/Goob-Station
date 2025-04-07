// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Bed;
using Robust.Client.GameObjects;

namespace Content.Client.Bed;

public sealed class StasisBedSystem : VisualizerSystem<StasisBedVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, StasisBedVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite != null
            && AppearanceSystem.TryGetData<bool>(uid, StasisBedVisuals.IsOn, out var isOn, args.Component))
        {
            args.Sprite.LayerSetVisible(StasisBedVisualLayers.IsOn, isOn);
        }
    }
}

public enum StasisBedVisualLayers : byte
{
    IsOn,
}
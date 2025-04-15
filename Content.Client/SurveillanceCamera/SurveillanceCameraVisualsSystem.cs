// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.SurveillanceCamera;
using Robust.Client.GameObjects;

namespace Content.Client.SurveillanceCamera;

public sealed class SurveillanceCameraVisualsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurveillanceCameraVisualsComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, SurveillanceCameraVisualsComponent component,
        ref AppearanceChangeEvent args)
    {
        if (!args.AppearanceData.TryGetValue(SurveillanceCameraVisualsKey.Key, out var data)
            || data is not SurveillanceCameraVisuals key
            || args.Sprite == null
            || !args.Sprite.LayerMapTryGet(SurveillanceCameraVisualsKey.Layer, out int layer)
            || !component.CameraSprites.TryGetValue(key, out var state))
        {
            return;
        }

        args.Sprite.LayerSetState(layer, state);
    }
}
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 EmoGarbage404 <retron404@gmail.com>
// SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Gravity;
using Content.Shared.Power;
using Robust.Client.GameObjects;

namespace Content.Client.Gravity;

public sealed partial class GravitySystem : SharedGravitySystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SharedGravityGeneratorComponent, AppearanceChangeEvent>(OnAppearanceChange);
        InitializeShake();
    }

    /// <summary>
    /// Ensures that the visible state of gravity generators are synced with their sprites.
    /// </summary>
    private void OnAppearanceChange(EntityUid uid, SharedGravityGeneratorComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearanceSystem.TryGetData<PowerChargeStatus>(uid, PowerChargeVisuals.State, out var state, args.Component))
        {
            if (comp.SpriteMap.TryGetValue(state, out var spriteState))
            {
                var layer = args.Sprite.LayerMapGet(GravityGeneratorVisualLayers.Base);
                args.Sprite.LayerSetState(layer, spriteState);
            }
        }

        if (_appearanceSystem.TryGetData<float>(uid, PowerChargeVisuals.Charge, out var charge, args.Component))
        {
            var layer = args.Sprite.LayerMapGet(GravityGeneratorVisualLayers.Core);
            switch (charge)
            {
                case < 0.2f:
                    args.Sprite.LayerSetVisible(layer, false);
                    break;
                case >= 0.2f and < 0.4f:
                    args.Sprite.LayerSetVisible(layer, true);
                    args.Sprite.LayerSetState(layer, comp.CoreStartupState);
                    break;
                case >= 0.4f and < 0.6f:
                    args.Sprite.LayerSetVisible(layer, true);
                    args.Sprite.LayerSetState(layer, comp.CoreIdleState);
                    break;
                case >= 0.6f and < 0.8f:
                    args.Sprite.LayerSetVisible(layer, true);
                    args.Sprite.LayerSetState(layer, comp.CoreActivatingState);
                    break;
                default:
                    args.Sprite.LayerSetVisible(layer, true);
                    args.Sprite.LayerSetState(layer, comp.CoreActivatedState);
                    break;
            }
        }
    }
}

public enum GravityGeneratorVisualLayers : byte
{
    Base,
    Core
}
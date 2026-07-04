// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <43253663+DogZeroX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <le0nel_1van@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Weapons.Ranged.Components;
using Content.Shared.Rounding;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    private void InitializeMagazineVisuals()
    {
        SubscribeLocalEvent<MagazineVisualsComponent, ComponentInit>(OnMagazineVisualsInit);
        SubscribeLocalEvent<MagazineVisualsComponent, AppearanceChangeEvent>(OnMagazineVisualsChange);
    }

    private void OnMagazineVisualsInit(Entity<MagazineVisualsComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite)) return;

        if (_sprite.LayerMapTryGet((ent, sprite), GunVisualLayers.Mag, out _, false))
        {
            _sprite.LayerSetRsiState((ent, sprite), GunVisualLayers.Mag, $"{ent.Comp.MagState}-{ent.Comp.MagSteps - 1}");
            _sprite.LayerSetVisible((ent, sprite), GunVisualLayers.Mag, false);
        }

        if (_sprite.LayerMapTryGet((ent, sprite), GunVisualLayers.MagUnshaded, out _, false))
        {
            _sprite.LayerSetRsiState((ent, sprite), GunVisualLayers.MagUnshaded, $"{ent.Comp.MagState}-unshaded-{ent.Comp.MagSteps - 1}");
            _sprite.LayerSetVisible((ent, sprite), GunVisualLayers.MagUnshaded, false);
        }
    }

    private void OnMagazineVisualsChange(Entity<MagazineVisualsComponent> ent, ref AppearanceChangeEvent args)
    {
        // tl;dr
        // 1.If no mag then hide it OR
        // 2. If step 0 isn't visible then hide it (mag or unshaded)
        // 3. Otherwise just do mag / unshaded as is
        var sprite = args.Sprite;

        if (sprite == null) return;

        if (!args.AppearanceData.TryGetValue(AmmoVisuals.MagLoaded, out var magloaded) ||
            magloaded is true)
        {
            if (!args.AppearanceData.TryGetValue(AmmoVisuals.AmmoMax, out var capacity))
            {
                capacity = ent.Comp.MagSteps;
            }

            if (!args.AppearanceData.TryGetValue(AmmoVisuals.AmmoCount, out var current))
            {
                current = ent.Comp.MagSteps;
            }

            var step = ContentHelpers.RoundToLevels((int)current, (int)capacity, ent.Comp.MagSteps);

            if (ent.Comp.ZeroNoAmmo && step == 0 && (int) current > 0) // Goobstation
                step = Math.Min(1, ent.Comp.MagSteps - 1);

            if (step == 0 && !ent.Comp.ZeroVisible)
            {
                if (_sprite.LayerMapTryGet((ent, sprite), GunVisualLayers.Mag, out _, false))
                {
                    _sprite.LayerSetVisible((ent, sprite), GunVisualLayers.Mag, false);
                }

                if (_sprite.LayerMapTryGet((ent, sprite), GunVisualLayers.MagUnshaded, out _, false))
                {
                    _sprite.LayerSetVisible((ent, sprite), GunVisualLayers.MagUnshaded, false);
                }

                return;
            }

            if (_sprite.LayerMapTryGet((ent, sprite), GunVisualLayers.Mag, out _, false))
            {
                _sprite.LayerSetVisible((ent, sprite), GunVisualLayers.Mag, true);
                _sprite.LayerSetRsiState((ent, sprite), GunVisualLayers.Mag, $"{ent.Comp.MagState}-{step}");
            }

            if (_sprite.LayerMapTryGet((ent, sprite), GunVisualLayers.MagUnshaded, out _, false))
            {
                _sprite.LayerSetVisible((ent, sprite), GunVisualLayers.MagUnshaded, true);
                _sprite.LayerSetRsiState((ent, sprite), GunVisualLayers.MagUnshaded, $"{ent.Comp.MagState}-unshaded-{step}");
            }
        }
        else
        {
            if (_sprite.LayerMapTryGet((ent, sprite), GunVisualLayers.Mag, out _, false))
            {
                _sprite.LayerSetVisible((ent, sprite), GunVisualLayers.Mag, false);
            }

            if (_sprite.LayerMapTryGet((ent, sprite), GunVisualLayers.MagUnshaded, out _, false))
            {
                _sprite.LayerSetVisible((ent, sprite), GunVisualLayers.MagUnshaded, false);
            }
        }
    }
}

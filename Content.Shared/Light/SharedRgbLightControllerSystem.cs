// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Light.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Light;

public abstract class SharedRgbLightControllerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RgbLightControllerComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, RgbLightControllerComponent component, ref ComponentGetState args)
    {
        args.State = new RgbLightControllerState(component.CycleRate, component.Layers);
    }

    public void SetLayers(EntityUid uid, List<int>? layers, RgbLightControllerComponent? rgb = null)
    {
        if (!Resolve(uid, ref rgb))
            return;

        rgb.Layers = layers;
        Dirty(uid, rgb);
    }

    public void SetCycleRate(EntityUid uid, float rate, RgbLightControllerComponent? rgb = null)
    {
        if (!Resolve(uid, ref rgb))
            return;

        rgb.CycleRate = Math.Clamp(0.01f, rate, 1); // lets not give people seizures
        Dirty(uid, rgb);
    }
}
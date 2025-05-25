// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Atmos;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class GasArtifactSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasArtifactComponent, ArtifactNodeEnteredEvent>(OnNodeEntered);
        SubscribeLocalEvent<GasArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnNodeEntered(EntityUid uid, GasArtifactComponent component, ArtifactNodeEnteredEvent args)
    {
        if (component.SpawnGas == null && component.PossibleGases.Count != 0)
        {
            var gas = component.PossibleGases[args.RandomSeed % component.PossibleGases.Count];
            component.SpawnGas = gas;
        }

        if (component.SpawnTemperature == null)
        {
            var temp = args.RandomSeed % component.MaxRandomTemperature - component.MinRandomTemperature +
                       component.MinRandomTemperature;
            component.SpawnTemperature = temp;
        }
    }

    private void OnActivate(EntityUid uid, GasArtifactComponent component, ArtifactActivatedEvent args)
    {
        if (component.SpawnGas == null || component.SpawnTemperature == null)
            return;

        var environment = _atmosphereSystem.GetContainingMixture(uid, false, true);
        if (environment == null)
            return;

        if (environment.Pressure >= component.MaxExternalPressure)
            return;

        var merger = new GasMixture(1) { Temperature = component.SpawnTemperature.Value };
        merger.SetMoles(component.SpawnGas.Value, component.SpawnAmount);

        _atmosphereSystem.Merge(environment, merger);
    }
}
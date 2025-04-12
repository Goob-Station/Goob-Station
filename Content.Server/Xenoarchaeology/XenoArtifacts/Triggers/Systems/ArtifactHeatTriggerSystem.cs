// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Jake Huxell <JakeHuxell@pm.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;
using Content.Shared.Interaction;
using Content.Shared.Temperature;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactHeatTriggerSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly ArtifactSystem _artifactSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ArtifactHeatTriggerComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<ArtifactHeatTriggerComponent, InteractUsingEvent>(OnUsing);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        List<Entity<ArtifactComponent>> toUpdate = new();
        var query = EntityQueryEnumerator<ArtifactHeatTriggerComponent, TransformComponent, ArtifactComponent>();
        while (query.MoveNext(out var uid, out var trigger, out var transform, out var artifact))
        {
            var environment = _atmosphereSystem.GetTileMixture((uid, transform));
            if (environment == null)
                continue;

            if (environment.Temperature < trigger.ActivationTemperature)
                continue;

            toUpdate.Add((uid, artifact));
        }

        foreach (var a in toUpdate)
        {
            _artifactSystem.TryActivateArtifact(a, null, a);
        }
    }

    private void OnAttacked(EntityUid uid, ArtifactHeatTriggerComponent component, AttackedEvent args)
    {
        if (!component.ActivateHotItems || !CheckHot(args.Used))
            return;
        _artifactSystem.TryActivateArtifact(uid, args.User);
    }

    private void OnUsing(EntityUid uid, ArtifactHeatTriggerComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!component.ActivateHotItems || !CheckHot(args.Used))
            return;
        args.Handled = _artifactSystem.TryActivateArtifact(uid, args.User);
    }

    private bool CheckHot(EntityUid usedUid)
    {
        var hotEvent = new IsHotEvent();
        RaiseLocalEvent(usedUid, hotEvent);
        return hotEvent.IsHot;
    }
}
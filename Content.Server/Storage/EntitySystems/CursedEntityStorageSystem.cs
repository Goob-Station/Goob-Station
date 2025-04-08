// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Storage.Components;
using Content.Shared.Audio;
using Content.Shared.Storage.Components;
using Robust.Shared.Random;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server.Storage.EntitySystems;

public sealed class CursedEntityStorageSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CursedEntityStorageComponent, StorageAfterCloseEvent>(OnClose);
    }

    private void OnClose(EntityUid uid, CursedEntityStorageComponent component, ref StorageAfterCloseEvent args)
    {
        if (!TryComp<EntityStorageComponent>(uid, out var storage))
            return;

        if (storage.Open || storage.Contents.ContainedEntities.Count <= 0)
            return;

        var lockers = new List<Entity<EntityStorageComponent>>();
        var query = EntityQueryEnumerator<EntityStorageComponent>();
        while (query.MoveNext(out var storageUid, out var storageComp))
        {
            lockers.Add((storageUid, storageComp));
        }

        lockers.RemoveAll(e => e.Owner == uid);

        if (lockers.Count == 0)
            return;

        var lockerEnt = _random.Pick(lockers).Owner;

        foreach (var entity in storage.Contents.ContainedEntities.ToArray())
        {
            _container.Remove(entity, storage.Contents);
            _entityStorage.AddToContents(entity, lockerEnt);
        }

        _audio.PlayPvs(component.CursedSound, uid, AudioHelpers.WithVariation(0.125f, _random));
    }
}
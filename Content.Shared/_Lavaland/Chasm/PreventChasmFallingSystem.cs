// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chasm;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Shared._Lavaland.Chasm;

public sealed class PreventChasmFallingSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreventChasmFallingComponent, BeforeChasmFallingEvent>(OnBeforeFall);
        SubscribeLocalEvent<InventoryComponent, BeforeChasmFallingEvent>(Relay);
    }

    private void OnBeforeFall(EntityUid uid, PreventChasmFallingComponent comp, ref BeforeChasmFallingEvent args)
    {
        args.Cancelled = true;
        var coordsValid = false;
        var coords = Transform(args.Entity).Coordinates;

        const int attempts = 20;
        var curAttempts = 0;
        while (!coordsValid)
        {
            curAttempts++;
            if (curAttempts > attempts)
                return; // Just to be safe from stack overflow
            
            var newCoords = new EntityCoordinates(Transform(args.Entity).ParentUid, coords.X + _random.NextFloat(-5f, 5f), coords.Y + _random.NextFloat(-5f, 5f));
            if (!_interaction.InRangeUnobstructed(args.Entity, newCoords, -1f) ||
                _lookup.GetEntitiesInRange<ChasmComponent>(newCoords, 1f).Count > 0)
                continue;

            _transform.SetCoordinates(args.Entity, newCoords);
            _transform.AttachToGridOrMap(args.Entity, Transform(args.Entity));
            _audio.PlayPvs("/Audio/Items/Mining/fultext_launch.ogg", args.Entity);
            if (args.Entity != uid)
                QueueDel(uid);

            coordsValid = true;
        }
    }

    private void Relay(EntityUid uid, InventoryComponent comp, ref BeforeChasmFallingEvent args)
    {
        if (!HasComp<ContainerManagerComponent>(uid))
            return;

        RelayEvent(uid, ref args);
    }

    private void RelayEvent(EntityUid uid, ref BeforeChasmFallingEvent ev)
    {
        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager))
            return;

        foreach (var container in containerManager.Containers.Values)
        {
            if (ev.Cancelled)
                break;

            foreach (var entity in container.ContainedEntities)
            {
                RaiseLocalEvent(entity, ref ev);
                if (ev.Cancelled)
                    break;
                RelayEvent(entity, ref ev);
            }
        }
    }
}
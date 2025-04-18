// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Fishfish458 <47410468+Fishfish458@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Júlio César Ueti <52474532+Mirino97@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 fishfish458 <fishfish458>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Verm <32827189+Vermidia@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Kitchen.Components;
using Content.Server.Popups;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Database;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Kitchen.EntitySystems;

namespace Content.Server.Access.Systems;

public sealed class IdCardSystem : SharedIdCardSystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly MicrowaveSystem _microwave = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IdCardComponent, BeingMicrowavedEvent>(OnMicrowaved);
    }

    private void OnMicrowaved(EntityUid uid, IdCardComponent component, BeingMicrowavedEvent args)
    {
        if (!component.CanMicrowave || !TryComp<MicrowaveComponent>(args.Microwave, out var micro) || micro.Broken)
            return;

        if (TryComp<AccessComponent>(uid, out var access))
        {
            float randomPick = _random.NextFloat();

            //basically, it does a small check to decide if its mango is to blow up -Space

            //roll microwave exploding -Space
            if (!micro.CanMicrowaveIdsSafely)
            {
                float explodeCheck = _random.NextFloat();

                if (explodeCheck <= micro.ExplosionChance)
                {
                    _microwave.Explode((args.Microwave, micro));
                    return;
                }

            }

            // then continue like normal -Space

            // if really unlucky, burn card
            if (randomPick <= 0.10f)
            {
                TryComp(uid, out TransformComponent? transformComponent);
                if (transformComponent != null)
                {
                    _popupSystem.PopupCoordinates(Loc.GetString("id-card-component-microwave-burnt", ("id", uid)),
                     transformComponent.Coordinates, PopupType.Medium);
                    EntityManager.SpawnEntity("FoodBadRecipe",
                        transformComponent.Coordinates);
                }
                _adminLogger.Add(LogType.Action, LogImpact.Medium,
                    $"{ToPrettyString(args.Microwave)} burnt {ToPrettyString(uid):entity}");
                EntityManager.QueueDeleteEntity(uid);
                return;
            }


            // If they're unlucky, brick their ID
            if (randomPick <= 0.4f)
            {
                _popupSystem.PopupEntity(Loc.GetString("id-card-component-microwave-bricked", ("id", uid)), uid);

                access.Tags.Clear();
                Dirty(uid, access);

                _adminLogger.Add(LogType.Action, LogImpact.Medium,
                    $"{ToPrettyString(args.Microwave)} cleared access on {ToPrettyString(uid):entity}");
            }
            else
            {
                _popupSystem.PopupEntity(Loc.GetString("id-card-component-microwave-safe", ("id", uid)), uid, PopupType.Medium);
            }

            // Give them a wonderful new access to compensate for everything
            var ids = _prototypeManager.EnumeratePrototypes<AccessLevelPrototype>().Where(x => x.CanAddToIdCard).ToArray();

            if (ids.Length == 0)
                return;

            var random = _random.Pick(ids);

            access.Tags.Add(random.ID);
            Dirty(uid, access);

            _adminLogger.Add(LogType.Action, LogImpact.Medium,
                    $"{ToPrettyString(args.Microwave)} added {random.ID} access to {ToPrettyString(uid):entity}");

        }
    }
}
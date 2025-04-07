// SPDX-FileCopyrightText: 2022 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Fishfish458 <47410468+Fishfish458@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 TekuNut <13456422+TekuNut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Theomund <34360334+Theomund@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 WlarusFromDaSpace <44726328+WlarusFromDaSpace@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 tmtmtl30 <53132901+tmtmtl30@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Access;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Power;
using Robust.Shared.Physics.Components;

namespace Content.Server.Doors.Systems;

public sealed class DoorSystem : SharedDoorSystem
{
    [Dependency] private readonly AirtightSystem _airtightSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoorBoltComponent, PowerChangedEvent>(OnBoltPowerChanged);
    }

    protected override void SetCollidable(
        EntityUid uid,
        bool collidable,
        DoorComponent? door = null,
        PhysicsComponent? physics = null,
        OccluderComponent? occluder = null)
    {
        if (!Resolve(uid, ref door))
            return;

        if (door.ChangeAirtight && TryComp(uid, out AirtightComponent? airtight))
            _airtightSystem.SetAirblocked((uid, airtight), collidable);

        // Pathfinding / AI stuff.
        RaiseLocalEvent(new AccessReaderChangeEvent(uid, collidable));

        base.SetCollidable(uid, collidable, door, physics, occluder);
    }

    private void OnBoltPowerChanged(Entity<DoorBoltComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered)
        {
            if (ent.Comp.BoltWireCut)
                SetBoltsDown(ent, true);
        }

        ent.Comp.Powered = args.Powered;
        Dirty(ent, ent.Comp);
        UpdateBoltLightStatus(ent);
    }
}
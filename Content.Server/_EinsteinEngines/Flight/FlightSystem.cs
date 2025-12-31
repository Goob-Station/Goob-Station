// SPDX-FileCopyrightText: 2024 Adeinitas <147965189+adeinitas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Danger Revolution! <142105406+DangerRevolution@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Timemaster99 <57200767+Timemaster99@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Flight;
using Content.Shared._EinsteinEngines.Flight.Events;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Audio.Systems;

namespace Content.Server._EinsteinEngines.Flight;
public sealed class FlightSystem : SharedFlightSystem
{
    
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlightComponent, FlightDoAfterEvent>(OnFlightDoAfter);
    }
    protected override void OnToggleFlight(EntityUid uid, FlightComponent component, ToggleFlightEvent args)
    {
        if (component.On)
        {
            ToggleActive(uid, false, component);
            return;
        }

        if (!CanFly(uid, component))
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            component.ActivationDelay,
            new FlightDoAfterEvent(),
            uid,
            target: uid)
        {
            BlockDuplicate = true,
            BreakOnDamage = true,
            NeedHand = true,
            MultiplyDelay = false
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }
    private void OnFlightDoAfter(EntityUid uid, FlightComponent component, FlightDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!CanFly(uid, component))
            return;

        ToggleActive(uid, true, component);
        args.Handled = true;
    }


}

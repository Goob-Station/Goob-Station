// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Silo;
using Content.Server.Lathe;
using Content.Server.Station.Components;
using Content.Shared._Goobstation.Silo;
using Content.Shared.DeviceLinking;
using Content.Shared.Lathe;
using Content.Shared.Materials;
using Robust.Server.GameStates;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Goobstation.Server.Silo;

public sealed class SiloSystem : SharedSiloSystem
{
    [Dependency] private readonly LatheSystem _lathe = default!;
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BecomesStationComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SiloComponent, MaterialAmountChangedEvent>(OnMaterialAmountChanged);
        SubscribeLocalEvent<SiloComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SiloComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<SiloComponent> ent, ref ComponentStartup args)
    {
        _pvs.AddGlobalOverride(ent);
    }

    private void OnShutdown(Entity<SiloComponent> ent, ref ComponentShutdown args)
    {
        _pvs.RemoveGlobalOverride(ent);
    }

    private void OnMaterialAmountChanged(Entity<SiloComponent> ent, ref MaterialAmountChangedEvent args)
    {
        // Spawning a timer because SetUiState in UpdateUserInterfaceState is being networked before
        // silo's MaterialStorageComponent state gets handled.
        // That causes lathe ui recipe list to not update properly.
        Timer.Spawn(20,
            () =>
            {
                if (!TryComp(ent, out DeviceLinkSourceComponent? source))
                    return;

                foreach (var utilizerSet in source.Outputs.Where(x => x.Key == SourcePort).Select(x => x.Value))
                {
                    foreach (var utilizer in utilizerSet)
                    {
                        if (TryComp(utilizer, out LatheComponent? lathe))
                            _lathe.UpdateUserInterfaceState(utilizer, lathe);
                    }
                }
            });
    }

    private void OnMapInit(Entity<BecomesStationComponent> ent, ref MapInitEvent args)
    {
        var siloQuery =
            AllEntityQuery<SiloComponent, MaterialStorageComponent, TransformComponent, DeviceLinkSourceComponent>();

        Entity<DeviceLinkSourceComponent>? silo = null;

        while (siloQuery.MoveNext(out var siloEnt, out _, out _, out var siloXform, out var source))
        {
            if (siloXform.GridUid != ent)
                continue;

            silo = (siloEnt, source);
            break;
        }

        if (silo == null)
            return;

        var utilizerQuery = AllEntityQuery<SiloUtilizerComponent, MaterialStorageComponent, TransformComponent,
            DeviceLinkSinkComponent>();
        while (utilizerQuery.MoveNext(out var utilizer, out _, out var storage, out var utilizerXform, out var sink))
        {
            if (!storage.ConnectToSilo)
                continue;

            if (utilizerXform.GridUid != ent)
                continue;

            DeviceLink.LinkDefaults(null, silo.Value, utilizer, silo.Value.Comp, sink);
        }
    }
}
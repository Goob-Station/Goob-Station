// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
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

using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Shared.CCVar;
using Content.Shared.Shuttles.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.IntegrationTests.Tests.Station;

[TestFixture]
[TestOf(typeof(EmergencyShuttleSystem))]
public sealed class EvacShuttleTest
{
    /// <summary>
    /// Ensure that the emergency shuttle can be called, and that it will travel to centcomm
    /// </summary>
    [Test]
    public async Task EmergencyEvacTest()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { DummyTicker = true, Dirty = true });
        var server = pair.Server;
        var entMan = server.EntMan;
        var ticker = server.System<GameTicker>();

        // Dummy ticker tests should not have centcomm
        Assert.That(entMan.Count<StationCentcommComponent>(), Is.Zero);

        Assert.That(pair.Server.CfgMan.GetCVar(CCVars.GridFill), Is.False);
        pair.Server.CfgMan.SetCVar(CCVars.EmergencyShuttleEnabled, true);
        pair.Server.CfgMan.SetCVar(CCVars.GameDummyTicker, false);
        var gameMap = pair.Server.CfgMan.GetCVar(CCVars.GameMap);
        pair.Server.CfgMan.SetCVar(CCVars.GameMap, "Saltern");

        await server.WaitPost(() => ticker.RestartRound());
        await pair.RunTicksSync(25);
        Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.InRound));

        // Find the station, centcomm, and shuttle, and ftl map.

        Assert.That(entMan.Count<StationCentcommComponent>(), Is.EqualTo(1));
        Assert.That(entMan.Count<StationEmergencyShuttleComponent>(), Is.EqualTo(1));
        Assert.That(entMan.Count<StationDataComponent>(), Is.EqualTo(1));
        Assert.That(entMan.Count<EmergencyShuttleComponent>(), Is.EqualTo(1));
        Assert.That(entMan.Count<FTLMapComponent>(), Is.EqualTo(0));

        var station = (Entity<StationCentcommComponent>) entMan.AllComponentsList<StationCentcommComponent>().Single();
        var data = entMan.GetComponent<StationDataComponent>(station);
        var shuttleData = entMan.GetComponent<StationEmergencyShuttleComponent>(station);

        var saltern = data.Grids.First(x => !entMan.HasComponent<Content.Server._Lavaland.Procedural.Components.LavalandStationComponent>(x)); // Lavaland change - ignore lavaland outpost
        Assert.That(entMan.HasComponent<MapGridComponent>(saltern));

        var shuttle = shuttleData.EmergencyShuttle!.Value;
        Assert.That(entMan.HasComponent<EmergencyShuttleComponent>(shuttle));
        Assert.That(entMan.HasComponent<MapGridComponent>(shuttle));

        var centcomm = station.Comp.Entity!.Value;
        Assert.That(entMan.HasComponent<MapGridComponent>(centcomm));

        var centcommMap = station.Comp.MapEntity!.Value;
        Assert.That(entMan.HasComponent<MapComponent>(centcommMap));
        Assert.That(server.Transform(centcomm).MapUid, Is.EqualTo(centcommMap));

        var salternXform = server.Transform(saltern);
        Assert.That(salternXform.MapUid, Is.Not.Null);
        Assert.That(salternXform.MapUid, Is.Not.EqualTo(centcommMap));

        var shuttleXform = server.Transform(shuttle);
        Assert.That(shuttleXform.MapUid, Is.Not.Null);
        Assert.That(shuttleXform.MapUid, Is.EqualTo(centcommMap));

        // All of these should have been map-initialized.
        var mapSys = entMan.System<SharedMapSystem>();
        Assert.That(mapSys.IsInitialized(centcommMap), Is.True);
        Assert.That(mapSys.IsInitialized(salternXform.MapUid), Is.True);
        Assert.That(mapSys.IsPaused(centcommMap), Is.False);
        Assert.That(mapSys.IsPaused(salternXform.MapUid!.Value), Is.False);

        EntityLifeStage LifeStage(EntityUid uid) => entMan.GetComponent<MetaDataComponent>(uid).EntityLifeStage;
        Assert.That(LifeStage(saltern), Is.EqualTo(EntityLifeStage.MapInitialized));
        Assert.That(LifeStage(shuttle), Is.EqualTo(EntityLifeStage.MapInitialized));
        Assert.That(LifeStage(centcomm), Is.EqualTo(EntityLifeStage.MapInitialized));
        Assert.That(LifeStage(centcommMap), Is.EqualTo(EntityLifeStage.MapInitialized));
        Assert.That(LifeStage(salternXform.MapUid.Value), Is.EqualTo(EntityLifeStage.MapInitialized));

        // Set up shuttle timing
        var shuttleSys = server.System<ShuttleSystem>();
        var evacSys = server.System<EmergencyShuttleSystem>();
        evacSys.TransitTime = shuttleSys.DefaultTravelTime; // Absolute minimum transit time, so the test has to run for at least this long
        // TODO SHUTTLE fix spaghetti

        var dockTime = server.CfgMan.GetCVar(CCVars.EmergencyShuttleDockTime);
        server.CfgMan.SetCVar(CCVars.EmergencyShuttleDockTime, 2);

        // Call evac shuttle.
        await pair.WaitCommand("callshuttle 0:02");
        await pair.RunSeconds(3);

        // Shuttle should have arrived on the station
        Assert.That(shuttleXform.MapUid, Is.EqualTo(salternXform.MapUid));

        await pair.RunSeconds(2);

        // Shuttle should be FTLing back to centcomm
        Assert.That(entMan.Count<FTLMapComponent>(), Is.EqualTo(1));
        var ftl = (Entity<FTLMapComponent>) entMan.AllComponentsList<FTLMapComponent>().Single();
        Assert.That(entMan.HasComponent<MapComponent>(ftl));
        Assert.That(ftl.Owner, Is.Not.EqualTo(centcommMap));
        Assert.That(ftl.Owner, Is.Not.EqualTo(salternXform.MapUid));
        Assert.That(shuttleXform.MapUid, Is.EqualTo(ftl.Owner));

        // Shuttle should have arrived at centcomm
        await pair.RunSeconds(shuttleSys.DefaultTravelTime);
        Assert.That(shuttleXform.MapUid, Is.EqualTo(centcommMap));

        // Round should be ending now
        Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.PostRound));

        server.CfgMan.SetCVar(CCVars.EmergencyShuttleDockTime, dockTime);
        pair.Server.CfgMan.SetCVar(CCVars.EmergencyShuttleEnabled, false);
        pair.Server.CfgMan.SetCVar(CCVars.GameMap, gameMap);
        await pair.CleanReturnAsync();
    }
}
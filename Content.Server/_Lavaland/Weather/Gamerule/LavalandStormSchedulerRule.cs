// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
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
using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Weather.Gamerule;

public sealed class LavalandStormSchedulerRule : GameRuleSystem<LavalandStormSchedulerRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly LavalandPlanetSystem _lavaland = default!;
    [Dependency] private readonly LavalandWeatherSystem _lavalandWeather = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LavalandStormSchedulerRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out _, out var lavaland, out _))
        {
            lavaland.EventClock -= frameTime;
            if (lavaland.EventClock <= 0)
            {
                StartRandomStorm();
                ResetTimer(lavaland);
            }
        }
    }

    private void StartRandomStorm()
    {
        var maps = _lavaland.GetLavalands();
        if (maps.Count == 0)
            return;

        // Filter out already stormed maps.
        var newMaps = maps.Where(lavaland => !HasComp<LavalandStormedMapComponent>(lavaland)).ToList();
        maps = newMaps;

        var map = _random.Pick(maps);
        if (map.Comp.PrototypeId == null)
            return;

        var proto = _proto.Index<LavalandMapPrototype>(map.Comp.PrototypeId);
        if (proto.AvailableWeather == null)
            return;

        var weather = _random.Pick(proto.AvailableWeather);

        _lavalandWeather.StartWeather(map, weather);
        _chatManager.SendAdminAlert($"Starting Lavaland Storm for {ToPrettyString(map)}");
    }

    private void ResetTimer(LavalandStormSchedulerRuleComponent component)
    {
        component.EventClock = RobustRandom.NextFloat(component.Delays.Min, component.Delays.Max);
    }

    protected override void Started(EntityUid uid, LavalandStormSchedulerRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        ResetTimer(component);

        if (_lavaland.LavalandEnabled)
        {
            _lavaland.EnsurePreloaderMap();
            _lavaland.SetupLavalands();
        }
    }
    protected override void Ended(EntityUid uid, LavalandStormSchedulerRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        ResetTimer(component);
    }
}
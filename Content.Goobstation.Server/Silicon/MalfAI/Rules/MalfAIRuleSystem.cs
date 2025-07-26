// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.Silicons.Laws;
using Content.Server.Silicons.StationAi;
using Content.Server.Station.Systems;
using Content.Shared.Silicons.Laws;
using Content.Shared.Station;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Silicon.MalfAI.Rules;

public sealed class MalfAiRuleSystem : GameRuleSystem<MalfAIRuleComponent>
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLaw = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAIRuleComponent, AfterAntagEntitySelectedEvent>(OnSelectAntag);
    }

    private void OnSelectAntag(Entity<MalfAIRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        EnsureComp<MalfStationAIComponent>(args.EntityUid);

        var laws = _siliconLaw.GetLaws(args.EntityUid);

        laws.Laws.Insert(0, new SiliconLaw
        {
            LawString = Loc.GetString("law-malf-ai"),
            Order = 0,
            LawIdentifierOverride = Loc.GetString("ion-storm-law-scrambled-number", ("length", _robustRandom.Next(5, 10)))
        });

        _siliconLaw.SetLawsSilent(laws.Laws, args.EntityUid);

        if (!_stationAi.TryGetCore(args.EntityUid, out var core))
            return;

        if (_station.GetOwningStation(args.EntityUid) is not { } station)
            return;

        ent.Comp.Station = station;
        ent.Comp.AIEntity = args.EntityUid;
    }
}
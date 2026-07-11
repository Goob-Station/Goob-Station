// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._MACRO.Thaven;
using Content.Server.Silicons.Laws;
using Content.Server.StationEvents.Components;
using Content.Shared._MACRO.Species.Thaven;
using Content.Shared.GameTicking.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Station.Components;

namespace Content.Server.StationEvents.Events;

public sealed class IonStormRule : StationEventSystem<IonStormRuleComponent>
{
    [Dependency] private readonly IonStormSystem _ionStorm = default!;

    protected override void Started(EntityUid uid, IonStormRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        var query = EntityQueryEnumerator<SiliconLawBoundComponent, TransformComponent, IonStormTargetComponent>();
        while (query.MoveNext(out var ent, out var lawBound, out var xform, out var target))
        {
            if (CompOrNull<StationMemberComponent>(xform.GridUid)?.Station != chosenStation)
                continue;

            _ionStorm.IonStormTarget((ent, lawBound, target));
        }

        var moodQuery = EntityQueryEnumerator<ThavenMoodsComponent, TransformComponent>();
        while (moodQuery.MoveNext(out var moodUid, out _, out var xform))
        {
            if (CompOrNull<StationMemberComponent>(xform.GridUid)?.Station != chosenStation)
                continue;

            var ev = new IonStormEvent();
            RaiseLocalEvent(moodUid, ref ev);
        }
    }
}
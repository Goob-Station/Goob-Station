using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;
using Content.Server.Pinpointer;

namespace Content.Goobstation.Server.Terror.Systems;

public sealed class TerrorQueenSenseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Listen for the action event fired when the queen triggers her sense ability.
        SubscribeLocalEvent<TerrorQueenSenseComponent, TerrorQueenSenseEvent>(OnSense);
    }

    private void OnSense(EntityUid uid, TerrorQueenSenseComponent comp, ref TerrorQueenSenseEvent args)
    {
        // uid = queen
        SenseSpiders(uid);
    }

    /// <summary>
    /// Called when the queen activates her "sense hive" ability.
    /// </summary>
    public void SenseSpiders(EntityUid queenUid)
    {
        var msg = BuildSpiderList();
        if (string.IsNullOrEmpty(msg))
        {
            _popup.PopupEntity("Your brood is silent. No living spiders detected.", queenUid, queenUid);
            return;
        }

        _popup.PopupEntity(msg, queenUid, queenUid);
    }

    private string BuildSpiderList()
    {
        var query = EntityQueryEnumerator<TerrorSpiderComponent, MobStateComponent>();
        var result = new System.Text.StringBuilder();
        var found = false;

        result.AppendLine("Living brood:");

        while (query.MoveNext(out var uid, out _, out var mob))
        {
            if (mob.CurrentState != MobState.Alive)
                continue;

            found = true;
            var loc = _navMap.GetNearestBeaconString(uid);
            var clean = FormattedMessage.RemoveMarkupOrThrow(loc);
            result.AppendLine($"{ToPrettyString(uid)} — {clean}");
        }

        return found ? result.ToString() : string.Empty;
    }
}

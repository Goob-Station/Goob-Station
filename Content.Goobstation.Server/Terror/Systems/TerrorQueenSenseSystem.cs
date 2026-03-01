using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Utility;
using Content.Server.Pinpointer;

namespace Content.Goobstation.Server.Terror.Systems;

/// <summary>
/// This system makes it so the queen gets a popup of all currently alive terror spider types and their locations.
/// </summary>
public sealed class TerrorQueenSenseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Listen for the action event fired when the queen triggers her sense ability.
        SubscribeLocalEvent<TerrorQueenSenseComponent, TerrorQueenSenseEvent>(OnSense);
    }

    /// <summary>
    /// Called when the queen activates her "sense hive" ability.
    /// </summary>
    private void OnSense(EntityUid uid, TerrorQueenSenseComponent comp, ref TerrorQueenSenseEvent args)
    {
        var msg = BuildSpiderList();
        if (string.IsNullOrEmpty(msg))
        {
            _popup.PopupEntity(
                Loc.GetString("queen-sense-none"),
                uid,
                uid);
            return;
        }

        _popup.PopupEntity(msg, uid, uid);
    }

    private string BuildSpiderList()
    {
        var query = EntityQueryEnumerator<TerrorSpiderComponent, MobStateComponent>();
        var result = new System.Text.StringBuilder();
        var found = false;

        result.AppendLine(Loc.GetString("queen-sense-header"));

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

using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerSenseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerSenseEvent>(OnSense);
    }

    private void OnSense(Entity<BloodsuckerComponent> ent, ref BloodsuckerSenseEvent args)
    {
        if (!TryGetCycle(out var cycle))
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-sense-no-cycle"),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        var remaining = cycle.Comp.TimeUntilCycle;
        var isDaytime = cycle.Comp.IsDaytime;

        string message;
        PopupType popupType;

        if (isDaytime)
        {
            if (remaining <= 10f)
            {
                message = Loc.GetString("bloodsucker-sense-day-ending-imminent",
                    ("seconds", (int) remaining));
                popupType = PopupType.Large;
            }
            else if (remaining <= 30f)
            {
                message = Loc.GetString("bloodsucker-sense-day-ending-soon",
                    ("seconds", (int) remaining));
                popupType = PopupType.Medium;
            }
            else
            {
                message = Loc.GetString("bloodsucker-sense-daytime",
                    ("seconds", (int) remaining));
                popupType = PopupType.MediumCaution;
            }
        }
        else
        {
            if (remaining <= cycle.Comp.WarnFinal)
            {
                message = Loc.GetString("bloodsucker-sense-night-ending-imminent",
                    ("seconds", (int) remaining));
                popupType = PopupType.LargeCaution;
            }
            else if (remaining <= cycle.Comp.WarnFirst)
            {
                message = Loc.GetString("bloodsucker-sense-night-ending-soon",
                    ("minutes", (int) (remaining / 60f)));
                popupType = PopupType.MediumCaution;
            }
            else
            {
                message = Loc.GetString("bloodsucker-sense-nighttime",
                    ("minutes", (int) (remaining / 60f)));
                popupType = PopupType.Small;
            }
        }

        _popup.PopupPredicted(message, ent.Owner, ent.Owner, popupType);
        args.Handled = true;
    }

    /// <summary>
    /// Returns the active day/night component if one exists.
    /// </summary>
    private bool TryGetCycle(out Entity<BloodsuckerDayNightComponent> cycle)
    {
        var query = EntityQueryEnumerator<BloodsuckerDayNightComponent>();
        if (query.MoveNext(out var uid, out var comp))
        {
            cycle = (uid, comp);
            return true;
        }
        cycle = default;
        return false;
    }
}

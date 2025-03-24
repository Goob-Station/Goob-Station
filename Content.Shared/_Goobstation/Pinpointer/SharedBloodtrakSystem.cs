using Content.Shared._Goobstation.Pinpointer;
using Content.Shared.Administration.Logs;
using Content.Shared.Emag.Systems;
using Content.Shared.Examine;

namespace Content.Shared._Gobostation.Pinpointer;

public abstract class SharedBloodtrakSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

    /// <summary>
    ///     Update direction from pinpointer to selected target (if it was set)
    /// </summary>
    protected virtual void UpdateDirectionToTarget(EntityUid uid, BloodtrakComponent? pinpointer = null)
    {

    }

    /// <summary>
    ///     Manually set distance from pinpointer to target
    /// </summary>
    public void SetDistance(EntityUid uid, Distance distance, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;

        if (distance == pinpointer.DistanceToTarget)
            return;

        pinpointer.DistanceToTarget = distance;
        Dirty(uid, pinpointer);
    }

    /// <summary>
    ///     Try to manually set pinpointer arrow direction.
    ///     If difference between current angle and new angle is smaller than
    ///     pinpointer precision, new value will be ignored and it will return false.
    /// </summary>
    public bool TrySetArrowAngle(EntityUid uid, Angle arrowAngle, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return false;

        if (pinpointer.ArrowAngle.EqualsApprox(arrowAngle, pinpointer.Precision))
            return false;

        pinpointer.ArrowAngle = arrowAngle;
        Dirty(uid, pinpointer);

        return true;
    }

    /// <summary>
    ///     Activate/deactivate pinpointer screen. If it has target it will start tracking it.
    /// </summary>
    public void SetActive(EntityUid uid, bool isActive, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;
        if (isActive == pinpointer.IsActive)
            return;

        pinpointer.IsActive = isActive;
        Dirty(uid, pinpointer);
    }


    /// <summary>
    ///     Toggle Pinpointer screen. If it has target it will start tracking it.
    /// </summary>
    /// <returns>True if pinpointer was activated, false otherwise</returns>
    public virtual bool TogglePinpointer(EntityUid uid, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return false;

        var isActive = !pinpointer.IsActive;
        SetActive(uid, isActive, pinpointer);
        return isActive;
    }
}

using Content.Goobstation.Common.Wizard.Events;

namespace Content.Client.UserInterface.Systems.Actions;

public sealed partial class ActionUIController
{
    public void SetSwapSecondaryTarget(EntityUid user, EntityUid? target, EntityUid action)
    {
        if (target == null || user == target)
        {
            _mark!.SetMark(null);
            EntityManager.RaisePredictiveEvent(new SetSwapSecondaryTarget(EntityManager.GetNetEntity(action), null));
            return;
        }

        _mark!.SetMark(target);
        EntityManager.RaisePredictiveEvent(new SetSwapSecondaryTarget(EntityManager.GetNetEntity(action), EntityManager.GetNetEntity(target.Value)));
    }
}
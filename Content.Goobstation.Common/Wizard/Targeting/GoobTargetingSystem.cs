using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Wizard.Targeting;

/// <summary>
/// this is a legit bridge between Goob Shared -> ActionUIController because i cant get shared dependancies to work in there for whatever reason..
/// </summary>
public sealed class GoobTargetingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;

    public event Action? StopTargeting;

    public override void Initialize()
    {
        SubscribeNetworkEvent<StopTargetingEvent>(OnStopTargeting);
    }

    private void OnStopTargeting(StopTargetingEvent msg, EntitySessionEventArgs args)
    {
        if(_netManager.IsClient)
            StopTargeting?.Invoke();
    }

    public void SetSwapSecondaryTarget(EntityUid user, EntityUid? target, EntityUid action)
    {
        if(_netManager.IsServer)
            return;

        if (target == null || user == target)
        {
            var markEvOne = new SetActionTargetMarkEvent(null);
            RaiseLocalEvent(user, markEvOne);
            RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), null));
            return;
        }

        var ev = new SetActionTargetMarkEvent(target);
        RaiseLocalEvent(user, ev);
        RaisePredictiveEvent(new SetSwapSecondaryTarget(GetNetEntity(action), GetNetEntity(target.Value)));
    }
}

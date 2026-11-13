using Content.Goobstation.Common.ServerCurrency;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds.Behaviors;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.ServerCurrency;

[DataDefinition]
public sealed partial class GiveGoobcoinsToKillerBehaviour : IThresholdBehavior
{
    [DataField]
    public int Amount = 1;

    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        if (cause is not {} killer || killer == owner)
            return;

        if (!system.EntityManager.TryGetComponent(killer, out ActorComponent? actor))
            return;

        IoCManager.Resolve<ICommonCurrencyManager>().AddCurrency(actor.PlayerSession.UserId, Amount);
    }
}

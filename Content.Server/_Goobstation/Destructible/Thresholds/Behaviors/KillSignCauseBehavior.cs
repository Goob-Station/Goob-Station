using Content.Server.Administration.Components;
using Content.Shared.Database;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    [Serializable]
    [DataDefinition]
    public sealed partial class KillSignCauseBehavior : IThresholdBehavior
    {
        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            if (cause == null)
                return;

            var causeVal = cause.Value;

            if (!system.EntityManager.TryGetComponent<KillSignComponent>(causeVal, out var killsignComp))
            {
                system.EntityManager.AddComponent<KillSignComponent>(causeVal);
                system._adminLogger.Add(LogType.Trigger, LogImpact.High, $"{system.EntityManager.ToPrettyString(causeVal):entity} was Killsigned because they broke a Christmas tree: {system.EntityManager.ToPrettyString(owner):entity}.");
            }
        }
    }
}

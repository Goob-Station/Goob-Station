using Content.Shared.Body.Components;
using Content.Shared.Gibbing.Events; // Shitmed Change
using JetBrains.Annotations;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class GibBehavior : IThresholdBehavior
    {
        [DataField] public GibType GibType = GibType.Gib; // Shitmed Change
        [DataField] public GibContentsOption GibContents = GibContentsOption.Drop; // Shitmed Change
        [DataField("recursive")] private bool _recursive = true;

        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            if (system.EntityManager.TryGetComponent(owner, out BodyComponent? body))
            {
                system.BodySystem.GibBody(owner, _recursive, body, gib: GibType, contents: GibContents); // Shitmed Change
            }
        }
    }
}

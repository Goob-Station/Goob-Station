using Content.Shared.Body.Part;
using JetBrains.Annotations;

// Leaving this one in the default namespace because I am afraid to test it
// in the Shitmed namespace lmao.
namespace Content.Server.Destructible.Thresholds.Behaviors;

[UsedImplicitly]
[DataDefinition]
public sealed partial class GibPartBehavior : IThresholdBehavior
{
    public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
    {
        if (!system.EntityManager.TryGetComponent(owner, out BodyPartComponent? part))
            return;

        system.BodySystem.GibPart(owner, part);
    }
}


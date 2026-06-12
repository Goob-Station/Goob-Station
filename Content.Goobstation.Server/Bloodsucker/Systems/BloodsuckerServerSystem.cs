using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Shared.Body.Components;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

/// <summary>
/// Handles server side of the Bloodsucker system, used only for communicating with client, honestly.
/// </summary>
public sealed class BloodsuckerServerSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodsuckerComponent, BloodstreamComponent>();

        while (query.MoveNext(out var uid, out var bloodsucker, out var bloodstream))
        {
            if (bloodstream.BloodSolution is not { } sol)
                continue;

            var newValue = (int) sol.Comp.Solution.Volume;

            if (bloodsucker.CurrentBlood != newValue)
            {
                bloodsucker.CurrentBlood = newValue;
                Dirty(uid, bloodsucker);
            }
        }
    }

}

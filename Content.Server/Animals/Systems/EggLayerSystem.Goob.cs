// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Animals.Components;

namespace Content.Server.Animals.Systems;

public sealed partial class EggLayerSystem
{
    /// <summary>
    /// Sets the cooldowns for the egg layer.
    /// </summary>
    public void SetCooldowns(Entity<EggLayerComponent> ent, float min, float max)
    {
        ent.Comp.EggLayCooldownMin = min;
        ent.Comp.EggLayCooldownMax = max;

        // Reset the running countdown so the new cooldown applies immediately.
        ent.Comp.NextGrowth = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(min, max));
    }
}

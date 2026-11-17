using Content.Goobstation.Shared.Slasher.Components;
using JetBrains.Annotations;

namespace Content.Goobstation.Shared.Slasher.Systems;

public sealed partial class SlasherFearSystem
{
    /// <summary>
    /// The slashers current fear meter.
    /// </summary>
    [PublicAPI]
    public float GetFear(EntityUid uid, SlasherFearComponent? fear = null)
    {
        return Resolve(uid, ref fear, false) ? fear.CurrentMeter : 0f;
    }

    /// <summary>
    /// How full the slashers fear meter is as a float.
    /// </summary>
    [PublicAPI]
    public float GetFearFraction(EntityUid uid, SlasherFearComponent? fear = null)
    {
        if (!Resolve(uid, ref fear, false) || fear.MaxMeter <= 0f)
            return 0f;

        return Math.Clamp(fear.CurrentMeter / fear.MaxMeter, 0f, 1f);
    }
}

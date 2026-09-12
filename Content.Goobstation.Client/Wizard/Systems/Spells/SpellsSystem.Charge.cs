using System.Numerics;
using Content.Client.Animations;

namespace Content.Goobstation.Client.Wizard.Systems.Spells;

public sealed partial class SpellsSystem
{
    protected override void ChargeEffectRelay(EntityUid uid)
    {
        // not predicted yet
        //if (!_timing.IsFirstTimePredicted || uid == EntityUid.Invalid)
        //    return;
        if (uid is not { Valid: true })
            return;

        var rays = _rays.DoRays(_xform.GetMapCoordinates(uid),
            Color.Yellow,
            Color.Fuchsia,
            10,
            15,
            minMaxRadius: new Vector2(3f, 6f),
            proto: "EffectRayCharge",
            server: false);

        if (rays == null)
            return;

        var track = EnsureComp<TrackUserComponent>(rays.Value);
        track.User = uid;
    }
}
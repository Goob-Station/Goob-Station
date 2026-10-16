using Content.Shared._ES.Camera;
using Content.Shared._Funkystation.Explosion;
using Robust.Shared.Player;

namespace Content.Server.Explosion.EntitySystems;

public sealed partial class ExplosionSystem
{
    [Dependency] private ESScreenshakeSystem _shake = default!;

    private static readonly ESScreenshakeParameters SmallExplosionShake = new(0.4f, 0.2f, 0.014f);
    private static readonly ESScreenshakeParameters LargeExplosionShake = new(0.6f, 0.05f, 0.014f);

    private void ExplosionScreenshake(Filter filter, bool small)
    {
        _shake.Screenshake(filter, small ? SmallExplosionShake : LargeExplosionShake, null);
    }

    private void RaiseExplosiveTriggered(EntityUid uid)
    {
        var ev = new ExplosiveTriggeredEvent();
        RaiseLocalEvent(uid, ref ev);
    }
}

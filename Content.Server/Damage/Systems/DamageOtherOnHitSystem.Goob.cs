using Content.Shared._ES.Camera;

namespace Content.Server.Damage.Systems;

public sealed partial class DamageOtherOnHitSystem
{
    [Dependency] private ESScreenshakeSystem _shake = default!;

    private static readonly ESScreenshakeParameters HitShake = new(0.35f, 1.4f, 0.014f);
}

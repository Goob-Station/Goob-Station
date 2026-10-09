using Content.Shared._ES.Camera;

namespace Content.Server.Projectiles;

public sealed partial class ProjectileSystem
{
    [Dependency] private ESScreenshakeSystem _shake = default!;

    private static readonly ESScreenshakeParameters HitShake = new(0.45f, 1.1f, 0.04f);
}

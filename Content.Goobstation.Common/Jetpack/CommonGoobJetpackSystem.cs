using System.Numerics;

namespace Content.Goobstation.Common.Jetpack;

public abstract class CommonGoobJetpackSystem : EntitySystem
{
    public abstract bool TryOverrideWishDir(EntityUid uid, out Vector2 wishDir);
}

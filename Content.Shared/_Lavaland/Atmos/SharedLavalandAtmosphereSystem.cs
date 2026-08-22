using Content.Shared.Atmos;

namespace Content.Shared._Lavaland.Atmos;

public abstract class SharedLavalandAtmosphereSystem : EntitySystem
{
    public virtual GasMixture? GetTileMixture(Entity<TransformComponent?> ent)
    {
        return null;
    }
}

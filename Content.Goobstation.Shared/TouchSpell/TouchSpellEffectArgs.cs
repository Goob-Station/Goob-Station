using Content.Shared.EntityEffects;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.TouchSpell;

public record class TouchSpellEffectArgs : EntityEffectBaseArgs
{
    public Entity<TouchSpellComponent> Origin;
    public EntityCoordinates? ClickLocation;

    public TouchSpellEffectArgs(EntityEffectBaseArgs original, Entity<TouchSpellComponent> origin, EntityCoordinates? clickLocation = null) : base(original)
    {
        Origin = origin;
        ClickLocation = clickLocation;
    }
}

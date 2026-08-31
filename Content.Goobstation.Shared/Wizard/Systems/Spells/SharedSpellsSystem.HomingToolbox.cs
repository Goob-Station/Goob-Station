using Content.Goobstation.Shared.Wizard.Events;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnHomingToolbox(HomingToolboxEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!ValidateLockOnAction(ev))
            return;

        var (_, mapCoords, spawnCoords, velocity) = GetProjectileData(ev.Performer);

        SpawnHomingProjectile(ev.Proto,
            spawnCoords,
            ev.Entity,
            ev.Performer,
            mapCoords,
            velocity,
            ev.ProjectileSpeed,
            true,
            _xform.ToMapCoordinates(ev.Target));

        ev.Handled = true;
    }
}
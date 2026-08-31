using Content.Shared.StatusEffect;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class SharedGoobSpellsSystem
{
    private void OnMagicMissile(MagicMissileEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var (coords, mapCoords, spawnCoords, velocity) = GetProjectileData(ev.Performer);

        var targets = _lookup.GetEntitiesInRange<StatusEffectsComponent>(coords, ev.Range, LookupFlags.Dynamic);
        var hasTargets = false;

        foreach (var (target, _) in targets)
        {
            if (target == ev.Performer)
                continue;

            if (_spectralQuery.HasComp(target))
                continue;

            hasTargets = true;

            SpawnHomingProjectile(ev.Proto,
                spawnCoords,
                target,
                ev.Performer,
                mapCoords,
                velocity,
                ev.ProjectileSpeed,
                false);
        }

        if (!hasTargets)
        {
            _popup.PopupClient(Loc.GetString(_locFailHomingNoTargets), ev.Performer);
            return;
        }

        ev.Handled = true;
    }
}
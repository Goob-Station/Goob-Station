namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class SharedGoobSpellsSystem
{
    private void OnDisableTech(DisableTechEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        // This doesn't invoke EmpPulse() because I don't want it to spawn emp effect and play pulse sound
        var coords = _xform.GetMapCoordinates(ev.Performer);
        foreach (var uid in _lookup.GetEntitiesInRange(coords, ev.Range))
        {
            if (_divineIntervention.TouchSpellDenied(uid)) // ipc chaplain meta i guess
                continue;

            _emp.TryEmpEffects(uid, ev.EnergyConsumption, TimeSpan.FromSeconds(ev.DisableDuration));
        }

        PredictedSpawnAttachedTo(ev.Effect, Transform(ev.Performer).Coordinates);

        ev.Handled = true;
    }
}
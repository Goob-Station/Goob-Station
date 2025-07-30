using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics;

public sealed partial class AnomalyPowerSystem
{
    /// <summary>
    ///     This function handles emulating the effects of a "Electrical Anomaly", using the caster as the "Anomaly",
    ///     while substituting their Psionic casting stats for "Severity and Stability".
    ///     This fires lightning bolts at random entities near the caster.
    /// </summary>
    private void DoElectricityAnomalyEffects(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args, bool overcharged = false)
    {
        if (args.Electricity is null)
            return;

        if (overcharged)
            ElectricitySupercrit(uid, component, args);
        else ElectricityPulse(uid, component, args);
    }

    private void ElectricitySupercrit(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        var range = args.Electricity!.Value.MaxElectrocuteRange * component.CurrentAmplification;

        _emp.EmpPulse(_xform.GetMapCoordinates(uid), range, args.Electricity!.Value.EmpEnergyConsumption, args.Electricity!.Value.EmpDisabledDuration);
        _lightning.ShootRandomLightnings(uid, range, args.Electricity!.Value.MaxBoltCount * (int) component.CurrentAmplification, arcDepth: (int) component.CurrentDampening);
    }

    private void ElectricityPulse(EntityUid uid, PsionicComponent component, AnomalyPowerActionEvent args)
    {
        var range = args.Electricity!.Value.MaxElectrocuteRange * component.CurrentAmplification;

        int boltCount = (int) MathF.Floor(MathHelper.Lerp(args.Electricity!.Value.MinBoltCount, args.Electricity!.Value.MaxBoltCount, component.CurrentAmplification));

        _lightning.ShootRandomLightnings(uid, range, boltCount);
    }
}
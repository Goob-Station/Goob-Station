using System.Diagnostics.CodeAnalysis;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Power.Components;

namespace Content.Shared._Afterlight.Silicons.Synths.Battery;

public sealed partial class SynthBatterySystem : EntitySystem
{
    [Dependency] private SharedBodySystem _body = default!;

    public bool TryGetBattery(Entity<BodyComponent?> ent, [NotNullWhen(true)] out Entity<BatteryComponent>? battery)
    {
        battery = null;

        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        foreach (var cell in _body.GetBodyOrganEntityComps<SynthPowerCellComponent>(ent))
        {
            if (!TryComp(cell.Owner, out BatteryComponent? batteryComp))
                continue;

            battery = (cell.Owner, batteryComp);
            return true;
        }

        return false;
    }

    public bool TryGetSynth(EntityUid cell, out Entity<SynthBatteryComponent> synth)
    {
        synth = default;

        if (!TryComp(cell, out OrganComponent? organ)
            || organ.Body is not { } body
            || !TryComp(body, out SynthBatteryComponent? synthBattery))
            return false;

        synth = (body, synthBattery);
        return true;
    }
}

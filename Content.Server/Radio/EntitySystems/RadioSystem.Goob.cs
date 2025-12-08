using Content.Shared.Access.Components;
using Content.Shared.PDA;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Server.Radio.EntitySystems;

public sealed partial class RadioSystem
{
    /// <summary>
    /// This handles the radio job icons that are displayed next to a players name when sending a message over radio
    /// </summary>
    /// <param name="EntityUID"></param>
    /// <returns></returns>
    private (ProtoId<JobIconPrototype>, string?) GetJobIcon(EntityUid ent)
    {
        if (_accessReader.FindAccessItemsInventory(ent, out var items))
        {
            foreach (var item in items)
            {
                // ID Card
                if (TryComp<IdCardComponent>(item, out var id))
                    return (id.JobIcon, id.LocalizedJobTitle);

                // PDA
                if (TryComp<PdaComponent>(item, out var pda)
                    && pda.ContainedId != null
                    && TryComp(pda.ContainedId, out id))
                    return (id.JobIcon, id.LocalizedJobTitle);
            }
        }

        if (HasComp<StationAiHeldComponent>(ent))
            return ("JobIconStationAi", Loc.GetString("job-name-station-ai"));

        if (HasComp<BorgChassisComponent>(ent) || HasComp<BorgBrainComponent>(ent))
            return ("JobIconBorg", Loc.GetString("job-name-borg"));

        return ("JobIconNoId", null);
    }
}

using Content.Shared.Access.Components;
using Content.Shared.PDA;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Radio.EntitySystems;

public sealed partial class RadioSystem
{
    // These are static vars rather than inlined in `TryGetJobIcon()` so that the YAML linter can verify that they actually exist.
    private static readonly ProtoId<JobIconPrototype> JobIconAI = new("JobIconStationAi");
    private static readonly ProtoId<JobIconPrototype> JobIconBorg = new("JobIconBorg");
    private static readonly ProtoId<JobIconPrototype> JobIconNoID = new("JobIconNoId");

    /// <summary>
    /// This handles getting the radio job icons that are displayed next to a players name when sending a message over radio.
    /// </summary>
    /// <param name="ent">The entity making a radio message.</param>
    /// <param name="jobIcon">
    /// The prototype ID of <paramref name="ent"/>'s job icon.<br/>
    /// If the method returns <see langword="false"/> then this will be <see langword="null"/>, otherwise it will always have a non-null value, defaulting to <c>"JobIconNoId"</c>.
    /// </param>
    /// <param name="jobName">The name of <paramref name="ent"/>'s job. If they don't <i>have</i> a job, (either none at all or "Unknown") then this will be <see langword="null"/>.</param>
    /// <returns>If <paramref name="ent"/> has a valid job icon, returns <see langword="true"/> and sets the out parameters. Otherwise returns <see langword="false"/>.</returns>
    private bool TryGetJobIcon(EntityUid ent, [NotNullWhen(true)] out ProtoId<JobIconPrototype>? jobIcon, out string? jobName)
    {
        jobIcon = jobName = null;

        // First things first, check if they're an AI or a borg. (They skip the `StatusIconComponent` check)
        if (HasComp<StationAiHeldComponent>(ent))
        {
            jobIcon = JobIconAI;
            jobName = Loc.GetString("job-name-station-ai");
        }
        else if (HasComp<BorgChassisComponent>(ent) || HasComp<BorgBrainComponent>(ent))
        {
            jobIcon = JobIconBorg;
            jobName = Loc.GetString("job-name-borg");
        }

        // If they don't have special silicon privileges, then only show a job icon in chat for entities who normally have one in-game.
        else if (!HasComp<StatusIconComponent>(ent))
        {
            return false;
        }

        // Try to get an ID card.
        if (jobIcon is null && _accessReader.FindAccessItemsInventory(ent, out var items))
        {
            IdCardComponent? idCard = null;
            foreach (var item in items)
            {
                // ID Card
                if (TryComp<IdCardComponent>(item, out var id))
                {
                    idCard = id;
                    break;
                }

                // PDA
                if (TryComp<PdaComponent>(item, out var pda)
                    && pda.ContainedId != null
                    && TryComp(pda.ContainedId, out id))
                {
                    idCard = id;
                    break;
                }
            }

            if (idCard != null)
            {
                jobIcon = idCard.JobIcon;
                jobName = idCard.LocalizedJobTitle;
            }
        }

        // If `jobIcon` is still null, set it to an 'Unknown' icon.
        jobIcon ??= JobIconNoID;

        return true;
    }
}

using Content.Shared._Imp.Drone;
using Content.Shared.Access.Systems;
using Content.Shared.PAI;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Shared.Radio;

public sealed class RadioJobIconSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;

    // These are static vars rather than being inlined so that the YAML linter can verify that they actually exist.
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
    public bool TryGetJobIcon(EntityUid ent, [NotNullWhen(true)] out ProtoId<JobIconPrototype>? jobIcon, out string? jobName)
    {
        // If they're an AI/borg/other silicon, they get to return early and skip the `StatusIconComponent` check.
        if (TryGetSiliconIcon(ent, out jobIcon, out jobName))
            return true;

        // If they don't have special silicon privileges, then only show a job icon in chat for entities who normally have one in-game.
        if (!HasComp<StatusIconComponent>(ent))
            return false;

        // Try to get the icon stuff from their ID card, if they have one.
        if (TryGetEquippedIDJob(ent, out jobIcon, out jobName))
            return true;

        // If none of the above methods found anything, set `jobIcon` to the 'Unknown'/'No ID' sprite.
        jobIcon = JobIconNoID;
        return true;
    }

    private bool TryGetSiliconIcon(EntityUid ent, [NotNullWhen(true)] out ProtoId<JobIconPrototype>? jobIcon, out string? jobName)
    {
        if (HasComp<StationAiHeldComponent>(ent))
        {
            jobIcon = JobIconAI;
            jobName = Loc.GetString("job-name-station-ai");
            return true;
        }
        if (HasComp<BorgChassisComponent>(ent)
            || HasComp<BorgBrainComponent>(ent)
            || HasComp<PAIComponent>(ent) // pAIs and Drones don't have radio access, but they can still get picked up by an intercom.
            || HasComp<DroneComponent>(ent))
        {
            jobIcon = JobIconBorg;
            jobName = Loc.GetString("job-name-borg");
            return true;
        }

        // If neither of the comp checks passed.
        jobIcon = jobName = null;
        return false;
    }

    private bool TryGetEquippedIDJob(EntityUid ent, [NotNullWhen(true)] out ProtoId<JobIconPrototype>? jobIcon, out string? jobName)
    {
        jobIcon = jobName = null;
        // Ideally this would only use `SharedIdCardSystem.TryFindIdCard()` rather than needing accessReader, but currently that doesn't check the offhand.
        if (!_accessReader.FindAccessItemsInventory(ent, out var items))
        {
            return false;
        }

        foreach (var item in items)
        {
            // Check if each item is an ID card, or if it's a PDA with an ID inside it.
            if (_idCardSystem.TryGetIdCard(item, out var idCard))
            {
                jobIcon = idCard.Comp.JobIcon;
                jobName = idCard.Comp.LocalizedJobTitle;
                return true;
            }
        }

        // Couldn't find an ID card
        return false;
    }
}

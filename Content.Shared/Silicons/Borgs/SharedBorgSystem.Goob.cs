using System.Linq;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Roles.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Silicons.Borgs;

public abstract partial class SharedBorgSystem
{
    private static readonly EntProtoId<MindRoleComponent>[] BorgDeconvertedRoles =
    {
        "MindRoleRevolutionary",
        "MindRoleCosmicCult",
        "MindRoleThrall",
        "MindRoleGangMember",
    };

    private void GoobDeconvertMind(EntityUid chassis, Entity<MindComponent> mind)
    {
        var present = mind.Comp.MindRoleContainer.ContainedEntities
            .Select(role => MetaData(role).EntityPrototype?.ID)
            .ToHashSet();

        var deconverted = false;
        foreach (var role in BorgDeconvertedRoles)
        {
            if (present.Contains(role.Id))
                deconverted |= _roles.MindRemoveRole((mind.Owner, mind.Comp), role);
        }

        if (deconverted)
            _popup.PopupEntity(Loc.GetString("borg-deconverted"), chassis, chassis, PopupType.Large);
    }
}

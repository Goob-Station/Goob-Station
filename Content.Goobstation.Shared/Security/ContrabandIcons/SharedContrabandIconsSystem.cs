using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Shared.Security.ContrabandIcons;

/// <summary>
/// This is responsible for updating the contraband status icon
/// </summary>
public abstract class SharedContrabandIconsSystem : EntitySystem
{
    private string StatusToIcon(ContrabandStatus status)
    {
        return status switch
        {
            ContrabandStatus.None => "ContrabandIconNone",
            ContrabandStatus.Contraband => "ContrabandIconContraband",
            _ => "ContrabandIconNone"
        };
    }
}

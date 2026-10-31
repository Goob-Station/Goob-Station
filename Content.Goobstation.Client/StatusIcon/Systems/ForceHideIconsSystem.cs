using Content.Client._Shitcode.Heretic;
using Content.Goobstation.Shared.StatusIcon.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;

namespace Content.Goobstation.Client.StatusIcon.Systems;

public sealed partial class ForceHideIconsSystem : EntitySystem
{
    [Dependency] private IPlayerManager _player = default!;

    [SubscribeLocalEvent(after: [typeof(GhoulSystem)])]
    private void OnGetStatusIcons(ref GetStatusIconsEvent args)
    {
        if (args.Uid != _player.LocalEntity && HasComp<ForceHideIconsComponent>(args.Uid))
            args.StatusIcons.Clear();
    }
}

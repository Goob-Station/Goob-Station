using System;
using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Administration.Systems;
using Content.Shared.Administration;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Pirate.Server.BwoinkFromConsole;

public sealed class BwoinkFromConsoleSystem : EntitySystem
{
    public static NetUserId SystemUserId { get; } = new NetUserId(Guid.Empty);
    [Dependency] private readonly IAdminManager _adminManager = default!;

    public void SendBwoink(string adminName, ICommonSession session, string message)
    {
        var bwonkEvent = new SharedBwoinkSystem.BwoinkTextMessage(session.UserId,
            SystemUserId,
            $"[color=purple]{adminName}[/color]: {message}");
        var admins = _adminManager.ActiveAdmins
            .Where(p => _adminManager.GetAdminData(p)?.HasFlag(AdminFlags.Adminhelp) ?? false)
            .Select(p => p.Channel)
            .ToList();

        foreach (var channel in admins)
        {
            RaiseNetworkEvent(bwonkEvent, channel);
        }

        if (!admins.Contains(session.Channel))
        {
            RaiseNetworkEvent(bwonkEvent, session.Channel);
        }
    }
}

using Robust.Server.Player;
using Content.Pirate.Shared.Components;
using Content.Pirate.Shared;
using Content.Shared.Eye;
using Robust.Shared.Console;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using System;
using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Shared.Administration;

namespace Content.Pirate.Server.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class TargetGhostCommand : IConsoleCommand
    {
        public string Command => "targetghost";
        public string Description => "Перетворює себе, гравця (ckey) або предмет (EntityUid) на ghost з TargetingGhost layer.";
        public string Help => "targetghost [ckey|uid]";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            EntityUid targetUid = default;
            var entManager = IoCManager.Resolve<IEntityManager>();
            var playerManager = IoCManager.Resolve<IPlayerManager>();

            if (args.Length == 0)
            {
                if (shell.Player?.AttachedEntity is EntityUid self)
                    targetUid = self;
                else
                {
                    shell.WriteLine("Немає прив'язаного entity.");
                    return;
                }
            }
            else if (args.Length == 1)
            {
                if (EntityUid.TryParse(args[0], out var uid) && entManager.EntityExists(uid))
                {
                    targetUid = uid;
                }
                else
                {
                    if (playerManager is IPlayerManager pm && pm.TryGetSessionByUsername(args[0], out var session) && session.AttachedEntity is EntityUid playerEnt)
                    {
                        targetUid = playerEnt;
                    }
                    else
                    {
                        shell.WriteLine($"Не знайдено entity для '{args[0]}'");
                        return;
                    }
                }
            }
            else
            {
                shell.WriteLine("Використання: targetghost [ckey|uid]");
                return;
            }

            if (!entManager.TryGetComponent<GhostTargetingComponent>(targetUid, out var ghostComp))
            {
                ghostComp = entManager.AddComponent<GhostTargetingComponent>(targetUid);
            }
            if (entManager.TryGetComponent<EyeComponent>(targetUid, out var eye))
            {
                var eyeSys = EntitySystem.Get<EyeSystem>();
                eyeSys.SetVisibilityMask(targetUid, eye.VisibilityMask | (int)VisibilityFlags.TargetingGhost, eye);
            }
            shell.WriteLine($"Entity {targetUid} перетворено на ghost з TargetingGhost layer.");
        }
    }

    [AdminCommand(AdminFlags.Admin)]
    public sealed class SetTargetGhostCommand : IConsoleCommand
    {
        public string Command => "settargetghost";
        public string Description => "Встановлює target ghost для entity (ckey або uid) на іншу entity (ckey або uid).";
        public string Help => "settargetghost <ckey|uid> <ckey|uid>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteLine("Використання: settargetghost <ckey|uid> <ckey|uid>");
                return;
            }

            var entManager = IoCManager.Resolve<IEntityManager>();
            var playerManager = IoCManager.Resolve<IPlayerManager>();

            EntityUid firstUid = default;
            EntityUid secondUid = default;

            if (EntityUid.TryParse(args[0], out var uid1) && entManager.EntityExists(uid1))
            {
                firstUid = uid1;
            }
            else if (playerManager is IPlayerManager pm1 && pm1.TryGetSessionByUsername(args[0], out var session1) && session1.AttachedEntity is EntityUid playerEnt1)
            {
                firstUid = playerEnt1;
            }
            else
            {
                shell.WriteLine($"Не знайдено entity для '{args[0]}'");
                return;
            }

            if (EntityUid.TryParse(args[1], out var uid2) && entManager.EntityExists(uid2))
            {
                secondUid = uid2;
            }
            else if (playerManager is IPlayerManager pm2 && pm2.TryGetSessionByUsername(args[1], out var session2) && session2.AttachedEntity is EntityUid playerEnt2)
            {
                secondUid = playerEnt2;
            }
            else
            {
                shell.WriteLine($"Не знайдено entity для '{args[1]}'");
                return;
            }

            if (!entManager.TryGetComponent<GhostTargetingComponent>(firstUid, out var ghostComp))
            {
                shell.WriteLine($"Entity {firstUid} не має GhostTargetingComponent.");
                return;
            }
            // Check if target has EyeComponent
            if (!entManager.TryGetComponent<EyeComponent>(secondUid, out var targetEye))
            {
                shell.WriteLine($"Entity {secondUid} не має EyeComponent. Target ghost не встановлено.");
                return;
            }
            ghostComp.Target = entManager.GetNetEntity(secondUid);
            shell.WriteLine($"Target ghost для entity {firstUid} встановлено на {secondUid}.");
        }
    }
}

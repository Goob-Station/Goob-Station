// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Robust.Shared.Player;
using Robust.Shared.Console;
using Robust.Shared.Enums;

// Goobstation - Stop Crit Hivemind
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Mobs.Systems;


namespace Content.Server.Chat.Commands
{
    [AnyCommand]
    internal sealed class CollectiveMindCommand : IConsoleCommand
    {
        public string Command => "cmsay";
        public string Description => "Send chat messages to the collective mind.";
        public string Help => "cmsay <text>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player is not ICommonSession player)
            {
                shell.WriteError("This command cannot be run from the server.");
                return;
            }

            if (player.Status != SessionStatus.InGame)
                return;

            if (player.AttachedEntity is not {} playerEntity)
            {
                shell.WriteError("You don't have an entity!");
                return;
            }

            // Goobstation - Stop Crit Hivemind
            // Check if the collective mind can be used in critical state
            var entityManager = IoCManager.Resolve<IEntityManager>();
            entityManager.TryGetComponent<CollectiveMindComponent>(playerEntity, out var mind);

            // Skip dead/critical check if CanUseInCrit is enabled
            if (mind != null && !mind.CanUseInCrit)
            {
                var mobStateSystem = EntitySystem.Get<MobStateSystem>();
                if (mobStateSystem.IsDead(playerEntity) || mobStateSystem.IsCritical(playerEntity))
                {
                    shell.WriteError("You cannot use the collective mind while dead or incapacitated!");
                    return;
                }
            }
            // Goobstation - End

            if (args.Length < 1)
                return;

            var message = string.Join(" ", args).Trim();
            if (string.IsNullOrEmpty(message))
                return;

            EntitySystem.Get<ChatSystem>().TrySendInGameICMessage(playerEntity, message, InGameICChatType.CollectiveMind, ChatTransmitRange.Normal);
        }
    }
}

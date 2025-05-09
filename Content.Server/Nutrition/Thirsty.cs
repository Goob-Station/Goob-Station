// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Console;

namespace Content.Server.Nutrition;

[AdminCommand(AdminFlags.Debug)]
public sealed class Thirsty : LocalizedEntityCommands
{
    public override string Command => "thirsty";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player;
        if (player == null)
        {
            shell.WriteError(Loc.GetString("cmd-nutrition-error-player"));
            return;
        }

        if (player.AttachedEntity is not {Valid: true} playerEntity)
        {
            shell.WriteError(Loc.GetString("cmd-nutrition-error-entity"));
            return;
        }

        if (!EntityManager.TryGetComponent(playerEntity, out ThirstComponent? thirst))
        {
            shell.WriteError(Loc.GetString("cmd-nutrition-error-component", ("comp", nameof(ThirstComponent))));
            return;
        }

        var thirstyThreshold = thirst.ThirstThresholds[ThirstThreshold.Parched];
        EntityManager.System<ThirstSystem>().SetThirst(playerEntity, thirst, thirstyThreshold);
    }
}
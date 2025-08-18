// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Ghost;
using Robust.Client.GameObjects;
using Robust.Shared.Console;

namespace Content.Client.Ghost;

public sealed class GhostToggleSelfVisibility : LocalizedEntityCommands
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override string Command => "toggleselfghost";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var attachedEntity = shell.Player?.AttachedEntity;
        if (!attachedEntity.HasValue)
            return;

        if (!EntityManager.HasComponent<GhostComponent>(attachedEntity))
        {
            shell.WriteError(Loc.GetString($"cmd-toggleselfghost-must-be-ghost"));
            return;
        }

        if (!EntityManager.TryGetComponent(attachedEntity, out SpriteComponent? spriteComponent))
            return;

        _sprite.SetVisible((attachedEntity.Value, spriteComponent), !spriteComponent.Visible);
    }
}
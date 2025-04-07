// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Ghost;
using Robust.Client.GameObjects;
using Robust.Shared.Console;

namespace Content.Client.Ghost;

public sealed class GhostToggleSelfVisibility : IConsoleCommand
{
    public string Command => "toggleselfghost";
    public string Description => "Toggles seeing your own ghost.";
    public string Help => "toggleselfghost";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var attachedEntity = shell.Player?.AttachedEntity;
        if (!attachedEntity.HasValue)
            return;

        var entityManager = IoCManager.Resolve<IEntityManager>();
        if (!entityManager.HasComponent<GhostComponent>(attachedEntity))
        {
            shell.WriteError("Entity must be a ghost.");
            return;
        }

        if (!entityManager.TryGetComponent(attachedEntity, out SpriteComponent? spriteComponent))
            return;

        spriteComponent.Visible = !spriteComponent.Visible;
    }
}
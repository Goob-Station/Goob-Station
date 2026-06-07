using Content.Server.Administration;
using Content.Server.Cloning;
using Content.Shared.Administration;
using Content.Shared.Inventory;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class CopyOutfitCommand : LocalizedEntityCommands
{
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    // No hands manipulation; rely on cloning system only

    public override string Command => "copyoutfit";

    public override string Description => Loc.GetString("cmd-copyoutfit-desc");

    public override string Help => Loc.GetString("cmd-copyoutfit-help");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 3)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var sourceNet) || !EntityManager.TryGetEntity(sourceNet, out var source))
        {
            shell.WriteError(Loc.GetString("shell-invalid-entity-id"));
            return;
        }

        if (!NetEntity.TryParse(args[1], out var targetNet) || !EntityManager.TryGetEntity(targetNet, out var target))
        {
            shell.WriteError(Loc.GetString("shell-invalid-entity-id"));
            return;
        }

        if (!EntityManager.TryGetComponent<InventoryComponent>(source, out var sourceInv) ||
            !EntityManager.TryGetComponent<InventoryComponent>(target, out var targetInv))
        {
            shell.WriteError(Loc.GetString("shell-entity-target-lacks-component", ("componentName", nameof(InventoryComponent))));
            return;
        }

        if (!bool.TryParse(args[2], out var delete))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-boolean"));
            return;
        }

        var slotEnum = _inventory.GetSlotEnumerator((target.Value, targetInv));
        while (slotEnum.NextItem(out _, out var slot))
        {
            _inventory.TryUnequip(target.Value, target.Value, slot.Name, out var itemRemoved, true, true, inventory: targetInv, reparent: !delete);
            if (delete && itemRemoved != null)
                EntityManager.QueueDeleteEntity(itemRemoved.Value);
        }

        // Intentionally ignore items in target hands

        _cloning.CopyEquipment((source.Value, sourceInv), (target.Value, targetInv), SlotFlags.All);

        // Do not copy items held in hands; only equipment via cloning system
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(
                CompletionHelper.Components<InventoryComponent>(args[0]),
                Loc.GetString("cmd-stripall-player-completion")),
            2 => CompletionResult.FromHintOptions(
                CompletionHelper.Components<InventoryComponent>(args[1]),
                Loc.GetString("cmd-stripall-player-completion")),
            3 => CompletionResult.FromHintOptions(
                CompletionHelper.Booleans,
                Loc.GetString("cmd-copyoutfit-delete-hint")),
            _ => CompletionResult.Empty
        };
    }
}

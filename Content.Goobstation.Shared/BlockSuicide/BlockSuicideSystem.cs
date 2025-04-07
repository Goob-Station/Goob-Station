using Content.Shared.Administration;
using Content.Shared.Popups;
using Robust.Shared.Console;

namespace Content.Goobstation.Shared.BlockSuicide;

public sealed partial class BlockSuicideSystem : EntitySystem
{
    public bool TryBlock(EntityUid user, IEntityManager entityManager, IConsoleShell shell)
    {
        if (!entityManager.HasComponent<BlockSuicideComponent>(user) || !entityManager.HasComponent<AdminFrozenComponent>(user))
            return false;

        var deniedMessage = Loc.GetString("suicide-command-denied");
        shell.WriteLine(deniedMessage);
        entityManager.System<SharedPopupSystem>() // mf i dont know why it has to be like this it just does cause its a command
            .PopupEntity(deniedMessage, user, user);
        return true;
    }
}


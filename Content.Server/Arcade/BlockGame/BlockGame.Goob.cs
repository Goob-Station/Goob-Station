using Content.Goobstation.Common.Arcade;

namespace Content.Server.Arcade.BlockGame;

public sealed partial class BlockGame
{
    private void RaiseGameOver(EntityUid player)
    {
        var ev = new BlockGameOverEvent(Points);
        _entityManager.EventBus.RaiseLocalEvent(player, ref ev);
    }
}

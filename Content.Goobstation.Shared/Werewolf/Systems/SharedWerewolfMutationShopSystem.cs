using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// This handles the mutation shop ability.
/// Opens the mutation shop
/// </summary>
public sealed class SharedWerewolfMutationShopSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfMutationShopComponent, OpenMutationShopEvent>(OnOpenMutationShop);
    }

    private void OnOpenMutationShop(Entity<WerewolfMutationShopComponent> entity, ref OpenMutationShopEvent args)
    {

    }
}

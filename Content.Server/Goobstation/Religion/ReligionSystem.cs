//Public Domain Code
using Content.Server.Bible.Components;
using Content.Server.GameTicking;
using Content.Shared.Goobstation.Religion;
using Content.Shared.Inventory;

namespace Content.Server.Goobstation.Religion;

public sealed class ReligionSystem: EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent args)
    {

        if (HasComp<BibleUserComponent>(args.Mob)) //Theoretically this can be used to let everyone spawn with the bible of their chosen faith
        {
            if (EntityManager.TryGetComponent(args.Mob, out ReligionComponent? mobReligion))
            {
                var bible = mobReligion.Type switch
                {
                    Shared.Goobstation.Religion.Religion.Atheist => "BibleAtheist",
                    Shared.Goobstation.Religion.Religion.Buddhist => "BibleBuddhist",
                    Shared.Goobstation.Religion.Religion.Christian => "Bible",
                    Shared.Goobstation.Religion.Religion.None => "Bible",
                };
                _inventorySystem.SpawnItemInSlot(args.Mob, "pocket1", bible, true, true);
            }
        }
    }
}

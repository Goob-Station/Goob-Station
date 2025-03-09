//Public Domain Code
using Content.Server.Bible.Components;
using Content.Shared._Goobstation.Religion;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;

namespace Content.Server._Goobstation.Religion;

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
                    Shared._Goobstation.Religion.Religion.Atheist => "BibleAtheist",
                    Shared._Goobstation.Religion.Religion.Buddhist => "BibleBuddhist",
                    Shared._Goobstation.Religion.Religion.Christian => "Bible",
                    Shared._Goobstation.Religion.Religion.None => "Bible",
                };
                _inventorySystem.SpawnItemInSlot(args.Mob, "pocket1", bible, true, true);
            }
        }
    }
}

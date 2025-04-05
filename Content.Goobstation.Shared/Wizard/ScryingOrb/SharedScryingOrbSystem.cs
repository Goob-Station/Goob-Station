using System.Linq;
using Content.Goobstation.Common.Wizard.ScryingOrb;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Wizard.ScryingOrb;

public abstract class SharedScryingOrbSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IsScryingOrbEquippedEvent>(OnIsScryingOrbEquipped);
    }

    private void OnIsScryingOrbEquipped(IsScryingOrbEquippedEvent ev)
    {
        ev.Equipped = IsScryingOrbEquipped(GetEntity(ev.User));
    }

    public bool IsScryingOrbEquipped(EntityUid uid)
    {
        var scryingOrbQuery = GetEntityQuery<ScryingOrbComponent>();
        if (_hands.EnumerateHeld(uid).Any(held => scryingOrbQuery.HasComponent(held)))
            return true;

        var enumerator = _inventory.GetSlotEnumerator(uid);
        while (enumerator.MoveNext(out var container))
        {
            if (scryingOrbQuery.HasComp(container.ContainedEntity))
                return true;
        }

        return false;
    }
}

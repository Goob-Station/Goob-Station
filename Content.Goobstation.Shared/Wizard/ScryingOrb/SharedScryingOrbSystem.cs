using System.Linq;
using Content.Goobstation.Common.Wizard.ScryingOrb;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Goobstation.Shared.Wizard.ScryingOrb;

public abstract class SharedScryingOrbSystem : EntitySystem
{
    public virtual bool IsScryingOrbEquipped(EntityUid uid)
    {
        return false;
    }
}

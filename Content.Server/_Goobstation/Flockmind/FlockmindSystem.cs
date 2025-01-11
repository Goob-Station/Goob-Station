using Content.Shared.Mind;
using Content.Server.Flash;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Stunnable;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;

namespace Content.Server.Flockmind
{
    public sealed partial class FlockmindSystem : EntitySystem
    {
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly FlashSystem _flash = default!;
        [Dependency] private readonly SharedBodySystem _body = default!;
        [Dependency] private readonly SharedStunSystem _stun = default!;
        [Dependency] private readonly StaminaSystem _stamina = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;

    }
}

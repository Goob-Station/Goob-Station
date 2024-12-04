using Content.Server._Lavaland.Mobs.Bosses.Components;
using Content.Server.Destructible;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Threading;

namespace Content.Server._Lavaland.Mobs.Bosses;

public sealed partial class MegafaunaSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // add these event handlers wherever necessary
        SubscribeLocalEvent<MegafaunaComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<MegafaunaComponent, DamageThresholdReached>(OnDeath);
    }

    [ValidatePrototypeId<EntityPrototype>] private const string CrusherPrototype = "BaseWeaponCrusher";

    public void OnAttacked<T>(EntityUid uid, T comp, ref AttackedEvent args) where T : MegafaunaComponent
    {
        var prot = Prototype(args.Used);
        if (prot == null)
            return;

        // check if the entity is a crusher or if any of it's parents are crusher
        // to account for crusher glaive, dagger and other stuff that you will not see :trollface:
        // generally makes it foolproof
        var pid = prot!.ID;
        var parents = prot!.Parents?.ToList() ?? new List<string>();

        if (pid != null && (pid != CrusherPrototype || parents.Contains(CrusherPrototype)))
            comp.CrusherOnly = false; // it's over...
    }

    public void OnDeath<T>(EntityUid uid, T comp, ref DamageThresholdReached args) where T : MegafaunaComponent
    {
        var coords = Transform(uid).Coordinates;

        if (comp.CancelToken != null)
            comp.CancelToken.Cancel();

        if (comp.Loot != null)
            Spawn(comp.Loot, coords);

        if (comp.CrusherOnly && comp.CrusherLoot != null)
            Spawn(comp.CrusherLoot, coords);
    }
}

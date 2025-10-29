using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Polymorph.Components;
using Content.Shared.Polymorph.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Goobstation.Shared.Morph;

/// <summary>
/// This handles shared systems for the morph antag
/// </summary>
public abstract class SharedMorphSystem : EntitySystem
{
    [Dependency] private readonly SharedChameleonProjectorSystem _chamleon = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MorphComponent, AttemptMeleeEvent>(OnAtack);
        SubscribeLocalEvent<ChameleonProjectorComponent, MorphEvent>(TryMorph);
    }

    private void OnAtack(EntityUid uid, MorphComponent component, ref AttemptMeleeEvent args)
    {
        //abort attack if morphed
        if (HasComp<ChameleonDisguisedComponent>(uid))
            args.Cancelled = true;
    }

    private void TryMorph(Entity<ChameleonProjectorComponent> ent, ref MorphEvent arg)
    {
        if (!_chamleon.TryDisguise(ent, arg.Performer, arg.Target))
            return;
        DisguiseInventory(ent, arg.Target);
    }

    public void DisguiseInventory(Entity<ChameleonProjectorComponent> ent, EntityUid target)
    {
        if(_net.IsClient)
            return;

        var user = ent.Comp.Disguised;

        if (!TryComp<ChameleonDisguisedComponent>(user, out var chamelion))
            return;
        var disguise = chamelion.Disguise;

        if (TryComp<HumanoidAppearanceComponent>(target,out var targetHumanoidAppearance))
        {
            EnsureComp<HumanoidAppearanceComponent>(disguise);
            _humanoidAppearance.CloneAppearance(target, disguise);
        }
        //TODO copy inventory
    }
}

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Network;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract class SharedVoidCloakSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidCloakHoodComponent, EntParentChangedMessage>(OnEntParentChanged);
        SubscribeLocalEvent<VoidCloakHoodComponent, EntityTerminatingEvent>(OnTerminating);

        SubscribeLocalEvent<VoidCloakComponent, InventoryRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);
    }

    private void OnCheckMagicItem(Entity<VoidCloakComponent> ent, ref InventoryRelayedEvent<CheckMagicItemEvent> args)
    {
        if (!ent.Comp.Transparent)
            args.Args.Handled = true;
    }

    private void OnTerminating(Entity<VoidCloakHoodComponent> ent, ref EntityTerminatingEvent args)
    {
        if (!TryComp(ent, out AttachedClothingComponent? attached))
            return;

        if (TerminatingOrDeleted(attached.AttachedUid))
            return;

        if (!TryComp(attached.AttachedUid, out VoidCloakComponent? comp))
            return;

        MakeCloakVisible(attached.AttachedUid, comp);
    }

    private void OnEntParentChanged(Entity<VoidCloakHoodComponent> ent, ref EntParentChangedMessage args)
    {
        if (!TryComp(ent, out AttachedClothingComponent? attached))
            return;

        if (TerminatingOrDeleted(attached.AttachedUid))
            return;

        if (!TryComp(attached.AttachedUid, out VoidCloakComponent? comp))
            return;

        if (args.OldParent == attached.AttachedUid) // If we equip the hood
            MakeCloakTransparent(attached.AttachedUid, comp);
        else // If we unequip hood (old parent is heretic in this case)
            MakeCloakVisible(attached.AttachedUid, comp);
    }

    private void MakeCloakTransparent(EntityUid cloak, VoidCloakComponent comp)
    {
        comp.Transparent = true;
        _clothing.SetEquippedPrefix(cloak, "transparent-");
        _appearance.SetData(cloak, VoidCloakVisuals.Transparent, true);

        if (_net.IsClient)
            return;

        EnsureComp<StripMenuInvisibleComponent>(cloak);
        UpdatePressureProtection(cloak, false);
    }

    private void MakeCloakVisible(EntityUid cloak, VoidCloakComponent comp)
    {
        comp.Transparent = false;
        _clothing.SetEquippedPrefix(cloak, null);
        _appearance.SetData(cloak, VoidCloakVisuals.Transparent, false);

        if (_net.IsClient)
            return;

        RemCompDeferred<StripMenuInvisibleComponent>(cloak);
        UpdatePressureProtection(cloak, true);
    }

    protected virtual void UpdatePressureProtection(EntityUid cloak, bool enabled)
    {
    }
}

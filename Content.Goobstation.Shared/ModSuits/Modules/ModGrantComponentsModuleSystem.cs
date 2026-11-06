using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Content.Shared.Item.ItemToggle;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModGrantComponentsModuleSystem : EntitySystem
{
    [Dependency] private ItemToggleSystem _itemToggle = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModGrantComponentsModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (TryComp<ModSuitComponent>(args.Suit, out var suit))
            Grant(ent, GetTarget(ent, (args.Suit, suit), args.Wearer));
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModGrantComponentsModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        Revoke(ent);
    }

    [SubscribeLocalEvent]
    private void OnWearerChanged(Entity<ModGrantComponentsModuleComponent> ent, ref ModSuitWearerChangedEvent args)
    {
        if (ent.Comp.Target != ModGrantTarget.Wearer || ent.Comp.GrantedTo == null)
            return;

        Revoke(ent);
        Grant(ent, args.NewWearer);
    }

    private EntityUid? GetTarget(Entity<ModGrantComponentsModuleComponent> ent, Entity<ModSuitComponent> suit, EntityUid? wearer)
    {
        return ent.Comp.Target switch
        {
            ModGrantTarget.Suit => suit.Owner,
            ModGrantTarget.Wearer => wearer,
            _ => ent.Comp.Slot is { } slot ? _modSuit.GetPart(suit, slot) : null,
        };
    }

    private void Grant(Entity<ModGrantComponentsModuleComponent> ent, EntityUid? target)
    {
        if (target is not { } uid || TerminatingOrDeleted(uid))
            return;

        _modSuit.GrantComponents(uid, ent.Comp.Components);
        ent.Comp.GrantedTo = uid;
        Dirty(ent);

        if (ent.Comp.ToggleItem)
            _itemToggle.TryActivate(uid, showPopup: false);
    }

    private void Revoke(Entity<ModGrantComponentsModuleComponent> ent)
    {
        if (ent.Comp.GrantedTo is not { } uid)
            return;

        ent.Comp.GrantedTo = null;
        Dirty(ent);

        if (TerminatingOrDeleted(uid))
            return;

        if (ent.Comp.ToggleItem)
            _itemToggle.TryDeactivate(uid, showPopup: false);

        _modSuit.RevokeComponents(uid, ent.Comp.Components);
    }
}

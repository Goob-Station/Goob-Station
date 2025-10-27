using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Clothing.Systems;

/// <summary>
/// This is used for the immortality talisman item
/// </summary>
public sealed class ImmortalityTalismanSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedGodmodeSystem _godmode = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ImmortalityTalismanComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ImmortalityTalismanComponent, GotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<ImmortalityTalismanComponent, GotEquippedEvent>(OnEquip);

        SubscribeLocalEvent<ActivateImmortalityTalismanActionEvent>(OnActivate);

        SubscribeLocalEvent<ImmortalityTalismanComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnModifierRefresh);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var quary = EntityQueryEnumerator<ImmortalityTalismanComponent>();
        while (quary.MoveNext(out var uid,out var talisman))
        {
            if (!talisman.Active)
                continue;
            if (_timing.CurTime < talisman.DisableAt)
                continue;

            EndImortality((uid,talisman));
        }
    }

    private void OnShutdown(Entity<ImmortalityTalismanComponent> ent, ref ComponentShutdown shutdown)
    {
        if (ent.Comp.Active)
            EndImortality(ent);
    }

    private void OnUnequip(Entity<ImmortalityTalismanComponent> ent,ref GotUnequippedEvent arg)
    {
        if(ent.Comp.Active)
            EndImortality(ent);

        _actionsSystem.RemoveAction(arg.Equipee, ent.Comp.ActionEntity);
    }

    private void OnEquip(Entity<ImmortalityTalismanComponent> ent,ref GotEquippedEvent arg)
    {
        ent.Comp.ActionEntity = _actionsSystem.AddAction(arg.Equipee,"ActionGainImmortality" );
    }

    private void OnActivate(ref ActivateImmortalityTalismanActionEvent arg)
    {
        if (arg.Handled
            || !_inventory.TryGetSlotEntity(arg.Performer, "neck", out var slotEntity)
            || !TryComp<ImmortalityTalismanComponent>(slotEntity, out var talisman))
            return;

        if (_timing.CurTime < talisman.Cooldown)
        {
            _popup.PopupPredicted(Loc.GetString("popup-Immortality-talisman-recharging"),arg.Performer,arg.Performer);
            return;
        }


        StartImortality((slotEntity.Value,talisman), arg.Performer);
        arg.Handled = true;
    }

    private void StartImortality(Entity<ImmortalityTalismanComponent> ent, EntityUid wearer)
    {
        if(!_netMan.IsClient) // not running this on clientside
            _godmode.EnableGodmode(wearer);

        ent.Comp.EntityGrantedImortality = wearer;
        ent.Comp.Active = true;
        ent.Comp.DisableAt = _timing.CurTime + ent.Comp.Duration;
        ent.Comp.Cooldown = _timing.CurTime + ent.Comp.CooldownDuration;

        _speedModifier.RefreshMovementSpeedModifiers(wearer);
        _popup.PopupPredicted(Loc.GetString("popup-Immortality-talisman-activated"),wearer,wearer);
    }

    private void EndImortality(Entity<ImmortalityTalismanComponent> ent)
    {
        if (ent.Comp.EntityGrantedImortality is null)
            return;
        var wearer = ent.Comp.EntityGrantedImortality.Value;

        _godmode.DisableGodmode(wearer);
        ent.Comp.Active = false;

        _speedModifier.RefreshMovementSpeedModifiers(wearer);
        _popup.PopupPredicted(Loc.GetString("popup-Immortality-talisman-ended"),wearer,wearer);
    }

    private void OnModifierRefresh(Entity<ImmortalityTalismanComponent> ent, ref InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (!ent.Comp.Active)
        {
            args.Args.ModifySpeed(1);
            return;
        }

        args.Args.ModifySpeed(0,true);
    }
}


using Content.Shared._Goobstation.Clothing.Components;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Clothing.Systems;

/// <summary>
///     System used for sealable clothing (like modsuits)
/// </summary>
public sealed partial class SealableClothingSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ToggleableClothingSystem _toggleableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SealableClothingControlComponent, ComponentRemove>(OnControlRemove);
        SubscribeLocalEvent<SealableClothingControlComponent, MapInitEvent>(OnControlMapInit);

        SubscribeLocalEvent<SealableClothingControlComponent, ClothingGotEquippedEvent>(OnControlEquip);
        SubscribeLocalEvent<SealableClothingControlComponent, ClothingGotUnequippedEvent>(OnControlUnequip);

        SubscribeLocalEvent<SealableClothingControlComponent, GetItemActionsEvent>(OnControlGetItemActions);
        SubscribeLocalEvent<SealableClothingControlComponent, SealClothingEvent>(OnControlSealEvent);
    }

    /// <summary>
    /// Removes action on component removal
    /// </summary>
    private void OnControlRemove(Entity<SealableClothingControlComponent> control, ref ComponentRemove args)
    {
        var comp = control.Comp;

        _actionsSystem.RemoveAction(comp.SealActionEntity);
    }

    /// <summary>
    /// Ensure actionEntity on map init
    /// </summary>
    private void OnControlMapInit(Entity<SealableClothingControlComponent> control, ref MapInitEvent args)
    {
        var (uid, comp) = control;
        _actionContainerSystem.EnsureAction(uid, ref comp.SealActionEntity, comp.SealAction);
    }

    /// <summary>
    /// Add/Remove wearer on clothing equip/unequip
    /// </summary>
    private void OnControlEquip(Entity<SealableClothingControlComponent> control, ref ClothingGotEquippedEvent args)
    {
        control.Comp.WearerEntity = args.Wearer;
        Dirty(control);
    }

    private void OnControlUnequip(Entity<SealableClothingControlComponent> control, ref ClothingGotUnequippedEvent args)
    {
        control.Comp.WearerEntity = null;
        Dirty(control);
    }

    /// <summary>
    /// Ensures seal action to player when it equip the seal control
    /// </summary>
    private void OnControlGetItemActions(Entity<SealableClothingControlComponent> control, ref GetItemActionsEvent args)
    {
        var (uid, comp) = control;

        if (comp.SealActionEntity == null || args.SlotFlags != comp.RequiredControlSlot)
            return;

        args.AddAction(comp.SealActionEntity.Value);
    }

    private void OnControlSealEvent(Entity<SealableClothingControlComponent> control, ref SealClothingEvent args)
    {
        var (uid, comp) = control;

        TryStartSealToggleProcess(control);
    }

    /// <summary>
    ///     Tries to start sealing process
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public bool TryStartSealToggleProcess(Entity<SealableClothingControlComponent> control)
    {
        var (uid, comp) = control;

        // Prevent sealing/unsealing if modsuit don't have wearer or already started process
        if (comp.WearerEntity == null || comp.IsInProcess)
            return false;

        // All parts required to be toggled to perform sealing
        if (_toggleableSystem.GetAttachedToggleStatus(uid) != ToggleableClothingAttachedStatus.AllToggled)
        {
            _popupSystem.PopupClient(Loc.GetString(comp.ToggleFailedPopup), uid, comp.WearerEntity);
            _audioSystem.PlayPredicted(comp.FailSound, uid, comp.WearerEntity);
            return false;
        }

        // Trying to get all clothing to seal
        var sealeableList = _toggleableSystem.GetAttachedClothingsList(uid);
        if (sealeableList == null)
            return false;

        foreach (var sealeable in sealeableList)
        {
            if (!HasComp<SealableClothingComponent>(sealeable))
            {
                _popupSystem.PopupEntity(Loc.GetString(comp.ToggleFailedPopup), uid, comp.WearerEntity.Value);
                _audioSystem.PlayPredicted(comp.FailSound, uid, comp.WearerEntity);

                comp.ProcessQueue.Clear();
                Dirty(control);

                return false;
            }

            comp.ProcessQueue.Enqueue(EntityManager.GetNetEntity(sealeable));
        }

        comp.IsInProcess = true;
        Dirty(control);

        NextSealProcess(control);

        return true;
    }

    private void NextSealProcess(Entity<SealableClothingControlComponent> control)
    {
        var (uid, comp) = control;

        if (comp.ProcessQueue.Count == 0)
        {
            comp.IsInProcess = false;
            comp.IsCurrentlySealed = !comp.IsCurrentlySealed;

            Dirty(control);
            return;
        }

        var processingPart = EntityManager.GetEntity(comp.ProcessQueue.Dequeue());
        Dirty(control);

        if (!TryComp<SealableClothingComponent>(processingPart, out var sealableComponent) || !comp.IsInProcess)
        {
            _popupSystem.PopupClient(Loc.GetString(comp.ToggleFailedPopup), uid, comp.WearerEntity);
            _audioSystem.PlayPredicted(comp.FailSound, uid, comp.WearerEntity);

            NextSealProcess(control);
            return;
        }

        // If part is sealed when control trying to seal - it should just skip this part
        if (sealableComponent.IsSealed != comp.IsCurrentlySealed)
        {
            NextSealProcess(control);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, sealableComponent.SealingTime, new SealClothingDoAfterEvent(), uid, target: processingPart)
        {
            NeedHand = false,
            RequireCanInteract = false,
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }
}

[Serializable, NetSerializable]
public sealed partial class SealClothingDoAfterEvent : SimpleDoAfterEvent
{
}


public sealed partial class SealClothingEvent : InstantActionEvent
{
}

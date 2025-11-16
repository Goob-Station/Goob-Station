using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Strip;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Shared.PowerCell;
using Content.Shared.Access.Systems;

namespace Content.Goobstation.Shared.ModSuits;

public abstract partial class SharedModSuitSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStrippableSystem _strippable = default!;
    [Dependency] private readonly SharedPowerCellSystem _cell = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedIdCardSystem _id = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeSuit();
        InitializeParts();
        InitializeModules();
    }

    public void UpdateUserInterface(EntityUid uid, ModSuitComponent component)
    {
        _ui.SetUiState(uid, ModSuitUiKey.Key, new RadialModBoundUiState());

        Dirty(uid, component);

        var state = new ModBoundUiState();

        foreach (var ent in component.ModuleContainer.ContainedEntities)
        {
            if (!TryComp<ModSuitModComponent>(ent, out var mod))
                continue;

            state.EquipmentStates.Add(GetNetEntity(ent), mod.Active);
        }

        _ui.SetUiState(uid, ModSuitMenuUiKey.Key, state);
    }
}

/// <summary>
/// Status of modsuit attachee
/// </summary>
[Serializable, NetSerializable]
public enum ModSuitAttachedStatus : byte
{
    NoneToggled,
    PartlyToggled,
    AllToggled
}
